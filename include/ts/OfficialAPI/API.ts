class API {
	public static BASE_PATH = "https://api.guildwars2.com/v2";

	public static RequestJson(path : string, apiToken : string|undefined = undefined, schemaVersion : string = 'latest') : Promise<any>
	{ return API.Request(path, apiToken, schemaVersion).then(r => r.json()); }

	public static Request(path : string, apiToken : string|undefined = undefined, schemaVersion : string = 'latest') : Promise<Response>
	{
		const headers = {
			'X-Schema-Version': schemaVersion,
		};
		if(apiToken) headers['Authorization'] = `Bearer ${apiToken}`;
		return fetch(API.BASE_PATH+path, {
			method: 'GET',
			headers: headers,
		});
	}
}
export default API;
