class API {
	public static BASE_PATH = "https://api.guildwars2.com/v2";

	public static async RequestJson(path : string, apiToken : string|undefined = undefined, schemaVersion : string = 'latest') : Promise<any>
	{
		const response = await API.Request(path, apiToken, schemaVersion);
		if (response.status !== 200) {
			let text = '';
			try {
				test = (await response.json()).text;
			}
			finally {
				throw new Error(`${path}: [${response.status}] ${response.statusText}: \n${text}`);
			}
		}
		return await response.json();
	}

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
