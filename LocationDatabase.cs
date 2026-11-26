using SQLite;

namespace LocationTracker;

[Table("Locations")]
public class LocationRecord
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string Latitude { get; set; } = "";
	public string Longitude { get; set; } = "";
}

public class LocationDatabase
{
	SQLiteAsyncConnection? _db;

	async Task InitAsync()
	{
		if (_db is not null)
			return;

		var path = Path.Combine(FileSystem.AppDataDirectory, "LocationDB.db");
		_db = new SQLiteAsyncConnection(path);
		await _db.CreateTableAsync<LocationRecord>();
	}

	public async Task<List<LocationRecord>> GetLocationsAsync()
	{
		await InitAsync();
		return await _db!.QueryAsync<LocationRecord>("SELECT Id, Latitude, Longitude FROM Locations");
	}

	public async Task<int> SaveLocationAsync(double latitude, double longitude)
	{
		await InitAsync();
		return await _db!.InsertAsync(new LocationRecord
		{
			Latitude = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
			Longitude = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
		});
	}
}
