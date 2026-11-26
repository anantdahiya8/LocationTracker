using System.Globalization;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationTracker;

public partial class MainPage : ContentPage
{
	static readonly TimeSpan SaveInterval = TimeSpan.FromMinutes(1);

	readonly LocationDatabase _database;
	IDispatcherTimer? _timer;

	public MainPage(LocationDatabase database)
	{
		InitializeComponent();
		_database = database;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		var current = await GetCurrentLocationAsync();

		await LoadPinsAsync();
		if (current is not null)
			LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(current, Distance.FromKilometers(1)));

		// Timers only fire while the app is in the foreground.
		_timer ??= CreateTimer();
		_timer.Start();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_timer?.Stop();
	}

	IDispatcherTimer CreateTimer()
	{
		var timer = Dispatcher.CreateTimer();
		timer.Interval = SaveInterval;
		timer.Tick += async (_, _) => await SaveCurrentLocationAsync();
		return timer;
	}

	static async Task<Location?> GetCurrentLocationAsync()
	{
		try
		{
			var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			if (status != PermissionStatus.Granted)
				return null;

			return await Geolocation.Default.GetLocationAsync(
				new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
		}
		catch (Exception)
		{
			return null;
		}
	}

	async Task SaveCurrentLocationAsync()
	{
		var location = await GetCurrentLocationAsync();
		if (location is null)
			return;

		await _database.SaveLocationAsync(location.Latitude, location.Longitude);
		await LoadPinsAsync();
	}

	async Task LoadPinsAsync()
	{
		LocationMap.MapElements.Clear();
		var locations = await _database.GetLocationsAsync();

		foreach (var item in locations)
		{
			if (double.TryParse(item.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
				double.TryParse(item.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
			{
				// Translucent fill so overlapping dots read darker, like a heat map.
				LocationMap.MapElements.Add(new Circle
				{
					Center = new Location(lat, lon),
					Radius = Distance.FromMeters(40),
					FillColor = Color.FromRgba(255, 0, 0, 90),
					StrokeColor = Colors.Transparent,
					StrokeWidth = 0
				});
			}
		}
	}

	async void OnMapClicked(object? sender, MapClickedEventArgs e)
	{
		await _database.SaveLocationAsync(e.Location.Latitude, e.Location.Longitude);
		await LoadPinsAsync();
	}
}
