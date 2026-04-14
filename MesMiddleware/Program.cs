using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;

Console.WriteLine("==========================================");
Console.WriteLine("  MES Middleware - SECS/GEM Host");
Console.WriteLine("  Connecting to MQTT broker...");
Console.WriteLine("==========================================");

// ── MQTT Setup ──────────────────────────────────────────
var mqttFactory = new MqttFactory();
var mqttClient = mqttFactory.CreateMqttClient();

var mqttOptions = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId("MES-Middleware")
    .Build();

await mqttClient.ConnectAsync(mqttOptions);
Console.WriteLine("[MQTT] Connected to broker on port 1883");

// ── Helper: publish JSON to MQTT ────────────────────────
async Task Publish(string topic, object payload)
{
    var json = JsonSerializer.Serialize(payload);
    var message = new MqttApplicationMessageBuilder()
        .WithTopic(topic)
        .WithPayload(Encoding.UTF8.GetBytes(json))
        .WithRetainFlag(true)
        .Build();

    await mqttClient.PublishAsync(message);
    Console.WriteLine($"[MQTT] >> {topic} | {json}");
}

// ── Simulate receiving SECS/GEM events from equipment ───
// In a real system this would be secs4net reading S6F11 messages
// Here we simulate the host side receiving the same state machine

Console.WriteLine("[MES] Starting equipment polling simulation...");
Console.WriteLine("[MES] Publishing events to MQTT (Tibco equivalent)");
Console.WriteLine("");

var states = new[] { "IDLE", "PROCESSING", "ALARM" };
var random = new Random();
var lotCounter = 1;
string currentState = "IDLE";
string? currentLot = null;

// Publish initial state
await Publish("mes/equipment/EQ-001/state", new {
    equipment_id = "EQ-001",
    state = currentState,
    lot_id = (string?)null,
    timestamp = DateTime.UtcNow
});

await Publish("mes/equipment/EQ-001/alarm", new {
    equipment_id = "EQ-001",
    alarm_active = false,
    alarm_code = (string?)null,
    message = (string?)null,
    timestamp = DateTime.UtcNow
});

// ── Main event loop ──────────────────────────────────────
while (true)
{
    try
    {
        if (currentState == "IDLE")
        {
            await Task.Delay(TimeSpan.FromSeconds(random.Next(3, 7)));
            currentLot = $"LOT-{lotCounter:D4}";
            lotCounter++;
            currentState = "PROCESSING";

            Console.WriteLine($"\n[MES] Equipment EQ-001 started processing {currentLot}");

            // Publish state change
            await Publish("mes/equipment/EQ-001/state", new {
                equipment_id = "EQ-001",
                state = currentState,
                lot_id = currentLot,
                timestamp = DateTime.UtcNow
            });

            // Publish lot start event
            await Publish("mes/lots/events", new {
                equipment_id = "EQ-001",
                lot_id = currentLot,
                event_type = "LOT_START",
                timestamp = DateTime.UtcNow
            });
        }
        else if (currentState == "PROCESSING")
        {
            await Task.Delay(TimeSpan.FromSeconds(random.Next(6, 12)));

            if (random.NextDouble() < 0.2)
            {
                // Trigger alarm
                currentState = "ALARM";
                Console.WriteLine($"\n[MES] ALARM on EQ-001 during {currentLot}");

                await Publish("mes/equipment/EQ-001/state", new {
                    equipment_id = "EQ-001",
                    state = currentState,
                    lot_id = currentLot,
                    timestamp = DateTime.UtcNow
                });

                await Publish("mes/equipment/EQ-001/alarm", new {
                    equipment_id = "EQ-001",
                    alarm_active = true,
                    alarm_code = "TEMP_HIGH",
                    message = "Temperature exceeded threshold — process paused",
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                // Complete lot normally
                currentState = "IDLE";
                Console.WriteLine($"\n[MES] Lot {currentLot} COMPLETE on EQ-001");

                await Publish("mes/lots/events", new {
                    equipment_id = "EQ-001",
                    lot_id = currentLot,
                    event_type = "LOT_COMPLETE",
                    timestamp = DateTime.UtcNow
                });

                await Publish("mes/equipment/EQ-001/state", new {
                    equipment_id = "EQ-001",
                    state = currentState,
                    lot_id = (string?)null,
                    timestamp = DateTime.UtcNow
                });

                currentLot = null;
            }
        }
        else if (currentState == "ALARM")
        {
            await Task.Delay(TimeSpan.FromSeconds(random.Next(4, 9)));
            currentState = "IDLE";
            Console.WriteLine($"\n[MES] Alarm cleared on EQ-001 — returning to IDLE");

            await Publish("mes/equipment/EQ-001/alarm", new {
                equipment_id = "EQ-001",
                alarm_active = false,
                alarm_code = (string?)null,
                message = "Alarm cleared",
                timestamp = DateTime.UtcNow
            });

            await Publish("mes/equipment/EQ-001/state", new {
                equipment_id = "EQ-001",
                state = currentState,
                lot_id = (string?)null,
                timestamp = DateTime.UtcNow
            });

            currentLot = null;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[MES] Error: {ex.Message}");
        await Task.Delay(2000);
    }
}