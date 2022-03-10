using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators {

	public struct UVSphere : IMeshGenerator {
        public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1);

		public int IndexCount => 6 * ResolutionU * ResolutionV;
		
		public int JobLength => ResolutionU + 1;

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

        public int Resolution { get; set; }
		int ResolutionV => 2 * Resolution;
		int ResolutionU => 4 * Resolution;

        public void Execute<S> (int u, S streams) where S : struct, IMeshStreams {
			int vi = (ResolutionV + 1) * u, ti = 2 * ResolutionV * (u - 1);

            var vertex = new Vertex();
			vertex.normal.y = 1f;
			vertex.tangent.w = -1f;

			vertex.position.x = sin(2f * PI * u / Resolution);
			vertex.position.z = -cos(2f * PI * u / Resolution);
			vertex.normal = vertex.position;

			float2 circle;
			circle.x = sin(2f * PI * u / ResolutionU);
			circle.y = cos(2f * PI * u / ResolutionU);
			vertex.tangent.xz = circle.yx;
			circle.y = -circle.y;
			vertex.texCoord0.x = (u - 0.5f) / ResolutionU;
			streams.SetVertex(vi, vertex);
			vi += 1;
			vertex.texCoord0.x = (float)u / ResolutionU;

			for (int v = 1; v <= ResolutionV; v++, vi++, ti += 2) {
				float circleRadius = sin(PI * v / ResolutionV);
				vertex.position.xz = circle * circleRadius;
				vertex.position.y = -cos(PI * v / ResolutionV);
				vertex.normal = vertex.position;
				vertex.texCoord0.y = (float)v / ResolutionV;
				streams.SetVertex(vi, vertex);

				if (u > 0) {
					streams.SetTriangle(
						ti + 0, vi + int3(-ResolutionV - 2, -ResolutionV - 1, -1)
					);
					streams.SetTriangle(
						ti + 1, vi + int3(-1, -ResolutionV - 1, 0)
					);
				}
			}
        }
    }

}