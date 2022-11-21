import readline from 'readline';
import fs from 'fs';

class TestUtilities {
	/** @var array<string, string> */
	public static CodesInvalid : Array<string>  = [];
	/** @var array<string, string> */
	public static CodesV1 : Array<string>       = [];
	/** @var array<string, string> */
	public static CodesV2 : Array<string>       = [];
	/** @var array<string, string> */
	public static CodesIngame : Array<string>   = [];
	/** @var array<string, string> */
	public static CodesV2Binary : Array<string> = [];

	static __construct_static() {
		let dict : Array<string>|null = null;
		let currentDict : string|null = null;
		let currentKey : string|null = null;
		let currentAccumulator = '';
		const file = readline.createInterface({
			input: fs.createReadStream('../../common/codes.ini'),
			output: process.stdout,
			terminal: false
		});
		file.on('line', (line_ : string) => {
			const comment = line_.indexOf(';');
			const line : string = (comment !== -1 ? line_.substring(0, comment) : line_).trim();
			if(line.length === 0) return;

			if(line.startsWith('[') && line.endsWith(']'))
			{
				const currentDict = line.substring(1, -1);
				switch (currentDict) {
					case "Invalid" : dict = TestUtilities.CodesInvalid; break;
					case "V1"      : dict = TestUtilities.CodesV1; break;
					case "V2"      : dict = TestUtilities.CodesV2; break;
					case "Ingame"  : dict = TestUtilities.CodesIngame; break;
					case "V2Binary": dict = TestUtilities.CodesV2Binary; break;
				};
			}
			else
			{
				if(currentDict !== 'V2Binary')
				{
					const [key, value] = line.split('=', 2);
					dict![key.trim()] = value.trim();
				}
				else if(line === "<end>")
				{
					dict![currentKey!] = currentAccumulator;
					currentAccumulator = '';
				}
				else
				{
					const split = line.indexOf('=');
					if(split !== -1)
					{
						currentKey = line.substring(0, split).trim();
					}
					else
					{
						currentAccumulator += line;
					}
				}
			}
		});
	}
}

TestUtilities.__construct_static();
export default TestUtilities;