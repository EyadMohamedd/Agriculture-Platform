/**
 * Agricultural Monitor System — MongoDB Seed Script
 *
 * Run with:
 *   mongosh mongodb://localhost:27017/AgriculturalMonitorDB seed.js
 *
 * NOTE: The application auto-seeds validation ranges and the default admin
 * account on first startup via AdminRepository.SeedDefaultValidationRangesAsync()
 * and the RunStartupTasksAsync() call in Program.cs.
 *
 * Use this script only to reset/repopulate a development database manually.
 *
 * Default admin credentials:
 *   Email:    admin@agrisystem.com
 *   Password: Admin@123
 *
 * The password hash below was generated with BCrypt work-factor 11.
 * Regenerate with: BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 11)
 */

// ── 1. Clear existing seed collections ───────────────────────────────────────
db.Users.deleteMany({ email: "admin@agrisystem.com" });
db.ValidationRanges.deleteMany({});
db.FarmValidationRanges.deleteMany({});

// ── 2. Default Admin user ─────────────────────────────────────────────────────
// IMPORTANT: Replace the password_hash with a freshly generated BCrypt hash
// if you change the default password. Do NOT store plaintext passwords.
db.Users.insertOne({
  name: "System Admin",
  email: "admin@agrisystem.com",
  phone: "+1234567890",
  // BCrypt hash of "Admin@123" with work factor 11
  // Re-generate in C#: BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 11)
  password_hash: "$2a$11$PLACEHOLDER_REPLACE_WITH_REAL_BCRYPT_HASH",
  role: "Admin",
  farm_location: null,
  created_at: new Date(),
  updated_at: new Date()
});

print("✓ Default admin user inserted (update password_hash with a real BCrypt hash)");

// ── 3. System-default validation ranges ──────────────────────────────────────
// Scientific defaults per sensor type.
// Farmers can override these per-farm via POST /api/farms/{farmId}/validation-ranges.
// Admins can update system defaults via PUT /api/admin/validation-ranges/{id}.
//
// Threshold hierarchy (AlertService checks in this order for each reading):
//   Critical  : value < critical_low  OR > critical_high
//   High      : value < warning_low   OR > warning_high
//   Medium    : value outside min_normal/max_normal but within warning range
//   Normal    : value within min_normal – max_normal  → no alert

db.ValidationRanges.insertMany([
  {
    // Soil temperature in °C
    // Most crops: 15–35°C optimal, outside 0–50°C is dangerous
    sensor_type: "temperature",
    min_normal:   15,
    max_normal:   35,
    warning_low:  10,
    warning_high: 40,
    critical_low: 0,
    critical_high: 50
  },
  {
    // Soil pH (dimensionless, 0–14 scale)
    // Most crops thrive at pH 6.0–7.5; below 4.0 or above 9.0 is toxic
    sensor_type: "ph",
    min_normal:   6.0,
    max_normal:   7.5,
    warning_low:  5.5,
    warning_high: 8.0,
    critical_low: 4.0,
    critical_high: 9.0
  },
  {
    // Soil moisture in % volumetric water content
    // Field capacity: 40–70%; below 20% plants wilt; above 90% causes root rot
    sensor_type: "moisture",
    min_normal:   40,
    max_normal:   70,
    warning_low:  30,
    warning_high: 80,
    critical_low: 20,
    critical_high: 90
  },
  {
    // Nitrogen (N) in mg/kg (ppm)
    // Medium fertility: 50–150 ppm; deficiency <10, toxicity >200
    sensor_type: "npk_n",
    min_normal:   50,
    max_normal:   150,
    warning_low:  30,
    warning_high: 180,
    critical_low: 10,
    critical_high: 200
  },
  {
    // Phosphorus (P) in mg/kg (ppm)
    // Adequate: 20–80 ppm; deficiency <5 stunts root growth
    sensor_type: "npk_p",
    min_normal:   20,
    max_normal:   80,
    warning_low:  10,
    warning_high: 100,
    critical_low: 5,
    critical_high: 120
  },
  {
    // Potassium (K) in mg/kg (ppm)
    // Adequate: 30–120 ppm; deficiency <10 affects fruit quality
    sensor_type: "npk_k",
    min_normal:   30,
    max_normal:   120,
    warning_low:  15,
    warning_high: 150,
    critical_low: 10,
    critical_high: 180
  },
  {
    // Rainfall in mm (per measurement interval)
    // Moderate: 40–70 mm; <20 mm drought risk; >90 mm flood risk
    sensor_type: "rainfall",
    min_normal:   40,
    max_normal:   70,
    warning_low:  30,
    warning_high: 80,
    critical_low: 20,
    critical_high: 90
  }
]);

print("✓ System-default validation ranges inserted (" +
      db.ValidationRanges.countDocuments({}) + " ranges)");

// ── 4. Indexes (match DatabaseIndexSetup.cs) ──────────────────────────────────
// These are also created programmatically on startup. Included here for
// completeness when resetting a dev database from scratch.

db.Users.createIndex({ email: 1 }, { unique: true, name: "idx_users_email_unique" });
db.Users.createIndex({ role: 1 }, { name: "idx_users_role" });

db.Farms.createIndex({ user_id: 1 }, { name: "idx_farms_user_id" });

db.Sensors.createIndex({ farm_id: 1 }, { name: "idx_sensors_farm_id" });
db.Sensors.createIndex({ status: 1 }, { name: "idx_sensors_status" });

db.SensorReadings.createIndex(
  { farm_id: 1, timestamp: -1 },
  { name: "idx_readings_farm_timestamp" }
);
db.SensorReadings.createIndex({ sensor_id: 1 }, { name: "idx_readings_sensor_id" });

db.Alerts.createIndex({ farm_id: 1 }, { name: "idx_alerts_farm_id" });
db.Alerts.createIndex({ is_resolved: 1 }, { name: "idx_alerts_is_resolved" });
db.Alerts.createIndex({ timestamp: 1 }, { name: "idx_alerts_timestamp" });

db.FarmValidationRanges.createIndex(
  { farm_id: 1, sensor_type: 1 },
  { unique: true, name: "idx_farm_ranges_unique" }
);

print("✓ MongoDB indexes created");
print("");
print("Seed complete. Start the API and login with:");
print("  POST /api/auth/login");
print('  { "email": "admin@agrisystem.com", "password": "Admin@123" }');
print("");
print("NOTE: The password_hash in Users is a placeholder.");
print("On first startup the application auto-creates the admin with a real BCrypt hash.");
print("Alternatively, run the app once, then re-seed only the ValidationRanges block.");
