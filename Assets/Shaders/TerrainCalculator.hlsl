void Terrain_float (
	float3 PositionIn, float NoiseIn,
	float Amplitude, float OffsetY,
	out float3 PositionOut
) {
	PositionOut = PositionIn + float3(0.0, Amplitude * (NoiseIn - OffsetY), 0.0);
}