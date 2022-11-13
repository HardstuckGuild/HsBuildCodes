<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Util;

class StringView implements \ArrayAccess {
	public string $Data;
	public int    $Pos;

	public function __construct(string $data, int $pos = 0) {
		$this->Data = $data;
		$this->Pos  = $pos;
	}

	public function NextByte() : int
	{ return ord($this->Data[$this->Pos++]); }

	public function NextChar() : string
	{ return $this->Data[$this->Pos++]; }

	public function NextUShortLE() : int
	{
		$val = current(unpack('v', $this->Data, $this->Pos));
		$this->Pos += 2;
		return $val;
	}

	public function Slice(int $shift) : StringView
	{ return new StringView($this->Data, $this->Pos + $shift); }

	public function LengthRemaining() : int
	{ return strlen($this->Data) - $this->Pos; }



	/** @param int $offset */
	public function offsetGet(mixed $offset) : int
	{ return ord($this->Data[$this->Pos + $offset]);   }

	/**
	 * @param int $offset
	 * @param int $value
	 */
	public function offsetSet(mixed $offset, mixed $value) : void
	{ $this->Data[$this->Pos + $offset] = $value;   }

	/** @param int $offset */
	public function offsetUnset(mixed $offset) : void
	{ throw new \Exception("cannot unset"); }
	
	/** @param int $offset */
	public function offsetExists(mixed $offset) : bool
	{ return 0 <= $offset && $offset < strlen($this->Data) - $this->Pos; }

	public function DebugPrint() : void
	{ print "\n".$this->Data."\n".str_repeat(' ', $this->Pos)."^- Current Pos\n"; }
}