class StringView {
	public Data : string;
	public Pos  : number;

	public constructor(data : string, pos : number = 0) {
		this.Data = data;
		this.Pos  = pos;
	}

	public NextByte() : number
	{ return this.Data.charCodeAt(this.Pos++); }

	public NextChar() : string
	{ return this.Data[this.Pos++]; }

	public Slice(shift : number) : StringView
	{ return new StringView(this.Data, this.Pos + shift); }

	public LengthRemaining() : number
	{ return this.Data.length - this.Pos; }

	public DebugPrint() : void
	{ console.log("\n"+this.Data+"\n"+' '.repeat(this.Pos)+"^- Current Pos\n"); }
}

export default StringView;
