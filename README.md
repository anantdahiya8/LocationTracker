# LocationTracker

A .NET MAUI app that displays the current location and saves location samples locally while the app is in the foreground. Tapping the map also saves a location sample.

## Requirements

- .NET MAUI workload for .NET 9
- A Google Maps SDK for Android API key to display map tiles on Android

The Android manifest intentionally contains no Maps API key. Add your own restricted key to the Android manifest locally when needed; do not commit keys or location database files.

Location samples are stored in the app's local data directory. A new install creates an empty database.
