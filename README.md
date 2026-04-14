# MES Integration Demo — SECS/GEM + MQTT + React

A full-stack Manufacturing Execution System (MES) integration demo built to demonstrate semiconductor manufacturing domain knowledge, including SECS/GEM equipment communication, pub/sub messaging middleware, and real-time dashboarding.

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│          Layer 1 — Equipment Simulator (Python)          │
│                  secsgem library                         │
│   Equipment State Machine → Lot Events → Alarm Events   │
└─────────────────────┬───────────────────────────────────┘
                      │ SECS/HSMS over TCP (port 5000)
                      │ (SEMI E37 standard)
┌─────────────────────▼───────────────────────────────────┐
│         Layer 2 — MES Middleware (C# .NET 10)            │
│   SECS/GEM Host │ Business Logic │ MQTT Publisher        │
│   Parses equipment messages → applies MES logic          │
└─────────────────────┬───────────────────────────────────┘
                      │ MQTT over WebSocket (port 9001)
                      │ Mosquitto broker — Tibco TIB/RV equivalent
┌─────────────────────▼───────────────────────────────────┐
│          Layer 3 — React Dashboard                       │
│   Live Equipment State │ Alarm Panel │ Lot Tracker       │
│   Subscribes to MQTT topics → real-time UI updates       │
└─────────────────────────────────────────────────────────┘
```

---

## What This Demonstrates

| Concept | Implementation |
|---|---|
| SECS/GEM equipment communication | Python `secsgem` simulator acting as equipment tool over SECS/HSMS TCP |
| MES host application | C# .NET app acting as GEM host, parsing state changes and lot events |
| Pub/sub messaging middleware | Mosquitto MQTT broker — same architectural pattern as Tibco TIB/RV |
| Real-time MES dashboard | React frontend subscribing to MQTT topics via WebSocket |
| Equipment state machine | OFF → IDLE → PROCESSING → ALARM → IDLE cycle |
| Lot tracking | LOT_START and LOT_COMPLETE events with lot ID traceability |
| Alarm management | TEMP_HIGH alarm trigger with automatic clear |

---

## MQTT Topic Structure

| Topic | Publisher | Description |
|---|---|---|
| `mes/equipment/EQ-001/state` | C# Middleware | Equipment state + current lot ID |
| `mes/equipment/EQ-001/alarm` | C# Middleware | Alarm active/clear with alarm code |
| `mes/lots/events` | C# Middleware | LOT_START and LOT_COMPLETE events |

---

## Tech Stack

- **Python 3** + `secsgem` — SECS/GEM equipment simulator
- **C# .NET 10** + `MQTTnet v4` — MES middleware / GEM host
- **Mosquitto 2.1** — MQTT broker (pub/sub messaging, Tibco TIB/RV equivalent)
- **React** + `mqtt.js` + `recharts` — Real-time dashboard

---

## Prerequisites

- Python 3.10+
- .NET 10 SDK
- Node.js 18+
- [Mosquitto MQTT Broker](https://mosquitto.org/download/)

---

## Getting Started

### 1. Clone the repo

```bash
git clone https://github.com/YOUR_USERNAME/MES_Demo.git
cd MES_Demo
```

### 2. Install Python dependencies

```bash
cd Simulator
pip install secsgem
```

### 3. Install React dependencies

```bash
cd dashboard
npm install
```

### 4. Configure Mosquitto

Create `mosquitto.conf` in your Mosquitto installation folder:

```
listener 1883
allow_anonymous true

listener 9001
protocol websockets
allow_anonymous true
```

---

## Running the Demo

You need **4 terminals running simultaneously**:

**Terminal 1 — MQTT Broker**
```bash
mosquitto -c mosquitto.conf -v
```

**Terminal 2 — Equipment Simulator**
```bash
cd Simulator
python equipment_simulator.py
```

**Terminal 3 — MES Middleware**
```bash
cd MesMiddleware
dotnet run
```

**Terminal 4 — React Dashboard**
```bash
cd dashboard
npm start
```

Open [http://localhost:3000](http://localhost:3000) to view the live dashboard.

---

## How It Works

### SECS/GEM Layer
The Python simulator implements a GEM equipment state machine using the `secsgem` library. It listens passively on TCP port 5000 and cycles through equipment states (IDLE → PROCESSING → ALARM), generating realistic lot and alarm events.

### MES Middleware
The C# .NET application acts as a SECS/GEM host, connecting to the simulator over SECS/HSMS (SEMI E37). It processes equipment events and publishes structured JSON messages to the Mosquitto MQTT broker — mirroring how real SemiCon MES systems use Tibco TIB/RV as the messaging backbone between CIM applications.

### Dashboard
The React frontend connects to Mosquitto via MQTT over WebSocket. It subscribes to all `mes/#` topics and updates the UI in real time — showing equipment state, active alarms, and a scrollable lot history table with event type badges.

---

## Relevance to SemiCon MES Roles

This project was built specifically to demonstrate readiness for CIM/MES engineering roles in semiconductor manufacturing:

- **SECS/GEM** — Industry standard equipment communication protocol (SEMI E5, E30, E37). This project implements the HSMS transport layer and GEM equipment model.
- **Pub/sub messaging** — Mosquitto MQTT mirrors the Tibco TIB/RV architecture used in production SemiCon fabs for inter-application messaging.
- **MES domain knowledge** — Lot tracking, equipment state management, alarm handling, and L2/L3 support context are all reflected in the implementation.

---

## Author

**Goh Kah Jin**
MES Engineer | Singapore
raymondgoh5275@gmail.com
