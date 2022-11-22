import * as readline from 'readline';
import * as fs from 'fs';
import { Assert } from '../../include/ts/Util/Static';

type Store = {
	[key : string] : string;
}

class TestUtilities {
	/** @var array<string, string> */
	public CodesInvalid : Store  = {};
	/** @var array<string, string> */
	public CodesV1 : Store       = {};
	/** @var array<string, string> */
	public CodesV2 : Store       = {};
	/** @var array<string, string> */
	public CodesIngame : Store   = {};
	/** @var array<string, string> */
	public CodesV2Binary : Store = {};

	public constructor() {
		let dict : Store|null = null;
		let currentDict : string|null = null;
		let currentKey : string|null = null;
		let currentAccumulator = '';
		const file = fs.readFileSync('../common/codes.ini', "utf-8");
		for(const line_ of file.split(/[\r\n]/)) {
			const comment = line_.indexOf(';');
			const line : string = (comment !== -1 ? line_.slice(0, comment) : line_).trim();
			if(line.length === 0) continue;

			if(line.startsWith('[') && line.endsWith(']'))
			{
				currentDict = line.slice(1, -1);
				switch (currentDict) {
					case "Invalid" : dict = this.CodesInvalid; break;
					case "V1"      : dict = this.CodesV1; break;
					case "V2"      : dict = this.CodesV2; break;
					case "Ingame"  : dict = this.CodesIngame; break;
					case "V2Binary": dict = this.CodesV2Binary; break;
				};
			}
			else
			{
				if(currentDict !== 'V2Binary')
				{
					const split = line.indexOf('=');
					const key = line.slice(0, split);
					const value = line.slice(split + 1);
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
						currentKey = line.slice(0, split).trim();
					}
					else
					{
						currentAccumulator += line;
					}
				}
			}
		}
	}
}

const TestUtilities_ = new TestUtilities();

export default TestUtilities_;