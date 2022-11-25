export function Base64Decode(base64 : string) : Uint8Array {
	return new Uint8Array(atob(base64).split('').map(c => c.charCodeAt(0)));
}

export function Base64Encode(binary : Uint8Array) : string {
	return btoa(String.fromCharCode(...binary));
}

export function Assert(expression : boolean, message : string|undefined = undefined, ...data : any[]) {
	//console.assert(expression, message, ...data);
	if(!expression) throw new Error("Assertion failed. "+message);
}