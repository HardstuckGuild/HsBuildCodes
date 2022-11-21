class BinaryView {
	public Data : ArrayBuffer;
	public Pos  : number;

	private _view : DataView;

	public constructor(data : ArrayBuffer, offset : number = 0) {
		this.Data = data;
		this.Pos  = offset;
		this._view = new DataView(data);
	}

	public ByteAt(offset : number) {
		return this._view.getUint8(this.Pos + offset);
	}

	public NextByte() : number {
		return this._view.getUint8(this.Pos++);
	}

	public NextUShortLE() : number {
		return this._view.getUint16(this.Pos++, true);
	}

	public Slice(offset : number) : BinaryView {
		return new BinaryView(this.Data, this.Pos + offset);
	}
}
export default BinaryView;