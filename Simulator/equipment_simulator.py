import secsgem.hsms
import secsgem.gem
import secsgem.common
import time
import random
import threading

class EquipmentSimulator(secsgem.gem.GemEquipmentHandler):
    def __init__(self):
        settings = secsgem.hsms.HsmsSettings(
            address="127.0.0.1",
            port=5000,
            connect_mode=secsgem.hsms.HsmsConnectMode.PASSIVE,
            device_type=secsgem.common.DeviceType.EQUIPMENT
        )
        super().__init__(settings)

        self.current_state = "IDLE"
        self.current_lot = None
        self.lot_counter = 1

    def simulate_loop(self):
        print("[SIMULATOR] Starting simulation loop...")
        while True:
            try:
                if self.current_state == "IDLE":
                    time.sleep(random.uniform(3, 6))
                    self.current_lot = f"LOT-{self.lot_counter:04d}"
                    self.lot_counter += 1
                    self.current_state = "PROCESSING"
                    print(f"[SIMULATOR] State: PROCESSING | Lot: {self.current_lot}")

                elif self.current_state == "PROCESSING":
                    time.sleep(random.uniform(5, 10))
                    if random.random() < 0.2:
                        self.current_state = "ALARM"
                        print(f"[SIMULATOR] State: ALARM | Lot: {self.current_lot}")
                    else:
                        self.current_state = "IDLE"
                        print(f"[SIMULATOR] State: IDLE | Lot: {self.current_lot} COMPLETE")
                        self.current_lot = None

                elif self.current_state == "ALARM":
                    time.sleep(random.uniform(4, 8))
                    self.current_state = "IDLE"
                    print(f"[SIMULATOR] State: IDLE | Alarm cleared")

            except Exception as e:
                print(f"[SIMULATOR] Error: {e}")
                time.sleep(2)

def main():
    print("=" * 50)
    print("  SECS/GEM Equipment Simulator")
    print("  Listening on 127.0.0.1:5000")
    print("=" * 50)

    sim = EquipmentSimulator()

    sim_thread = threading.Thread(target=sim.simulate_loop, daemon=True)
    sim_thread.start()

    sim.enable()
    print("[SIMULATOR] Waiting for host connection on port 5000...")

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\n[SIMULATOR] Shutting down...")
        sim.disable()

if __name__ == "__main__":
    main()