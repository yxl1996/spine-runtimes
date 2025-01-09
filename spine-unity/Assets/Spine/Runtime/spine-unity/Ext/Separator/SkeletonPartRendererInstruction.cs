#define SPINE_TRIANGLECHECK
#define SLOT_ALPHA_DISABLES_ATTACHMENT

namespace Spine.Unity
{
    public class SkeletonPartRendererInstruction
    {
        public SkeletonRendererInstruction skeletonRendererInstruction; 

		public bool immutableTriangles;
#if SPINE_TRIANGLECHECK
		public bool hasActiveClipping;
		public int rawVertexCount = -1;
        public int startSubmesh;
        public int endSubmesh;
        public int submeshCount;
        public int startSlot;
        public int endSlot;
        public int attachmentCount;
#endif

		public void Clear () {
#if SPINE_TRIANGLECHECK
			rawVertexCount = -1;
			hasActiveClipping = false;
            startSubmesh = -1;
            endSubmesh = -1;
            submeshCount = -1;
            startSlot = -1;
            endSlot = -1;
            attachmentCount = -1;
#endif
        }

		public void SetWithSubset (SkeletonRendererInstruction skeletonRendererInstruction, int startSubmesh, int endSubmesh) {
            this.skeletonRendererInstruction = skeletonRendererInstruction;
            this.startSubmesh = startSubmesh;
            this.endSubmesh = endSubmesh;
            
#if SPINE_TRIANGLECHECK
            int runningVertexCount = 0;
            int submeshCount = endSubmesh - startSubmesh;
            this.submeshCount = submeshCount;
			SubmeshInstruction[] instructionsItems = skeletonRendererInstruction.submeshInstructions.Items;
			for (int i = 0; i < submeshCount; i++) {
				SubmeshInstruction instruction = instructionsItems[startSubmesh + i];
                this.hasActiveClipping |= instruction.hasClipping;
                runningVertexCount += instruction.rawVertexCount; // vertexCount will also be used for the rest of this method.
            }
            this.rawVertexCount = runningVertexCount;
            
            this.startSlot = instructionsItems[startSubmesh].startSlot;
            this.endSlot = instructionsItems[endSubmesh - 1].endSlot;
            this.attachmentCount = endSlot - startSlot;
#endif
		}

		public void Set (SkeletonPartRendererInstruction other)
        {
            // 这里直接使用引用，所以要在外部保证每一次使用的skeletonRendererInstruction都是缓存
            this.skeletonRendererInstruction = other.skeletonRendererInstruction;
            this.immutableTriangles = other.immutableTriangles;

#if SPINE_TRIANGLECHECK
			this.hasActiveClipping = other.hasActiveClipping;
			this.rawVertexCount = other.rawVertexCount;
            this.startSubmesh = other.startSubmesh;
            this.endSubmesh = other.endSubmesh;
            this.submeshCount = other.submeshCount;
            this.startSlot = other.startSlot;
            this.endSlot = other.endSlot;
            this.attachmentCount = other.attachmentCount;
#endif
        }

		public static bool GeometryNotEqual (SkeletonPartRendererInstruction a, SkeletonPartRendererInstruction b) {
#if SPINE_TRIANGLECHECK
#if UNITY_EDITOR
			if (!UnityEngine.Application.isPlaying)
				return true;
#endif

			if (a.hasActiveClipping || b.hasActiveClipping) return true; // Triangles are unpredictable when clipping is active.

			// Everything below assumes the raw vertex and triangle counts were used. (ie, no clipping was done)
			if (a.rawVertexCount != b.rawVertexCount) return true;

			if (a.immutableTriangles != b.immutableTriangles) return true;

			int attachmentCountB = b.attachmentCount;
			if (a.attachmentCount != attachmentCountB) return true; // Bounds check for the looped storedAttachments count below.

			// Submesh count changed
			int submeshCountA = a.submeshCount;
			int submeshCountB = b.submeshCount;
			if (submeshCountA != submeshCountB) return true;

			// Submesh Instruction mismatch
			SubmeshInstruction[] submeshInstructionsItemsA = a.skeletonRendererInstruction.submeshInstructions.Items;
			SubmeshInstruction[] submeshInstructionsItemsB = b.skeletonRendererInstruction.submeshInstructions.Items;

			Attachment[] attachmentsA = a.skeletonRendererInstruction.attachments.Items;
			Attachment[] attachmentsB = b.skeletonRendererInstruction.attachments.Items;
			for (int i = b.startSlot; i < attachmentCountB; i++)
				if (!System.Object.ReferenceEquals(attachmentsA[i], attachmentsB[i])) return true;

			for (int i = b.startSubmesh; i < submeshCountB; i++) {
				SubmeshInstruction submeshA = submeshInstructionsItemsA[i];
				SubmeshInstruction submeshB = submeshInstructionsItemsB[i];

				if (!(
					submeshA.rawVertexCount == submeshB.rawVertexCount &&
					submeshA.startSlot == submeshB.startSlot &&
					submeshA.endSlot == submeshB.endSlot
					&& submeshA.rawTriangleCount == submeshB.rawTriangleCount &&
					submeshA.rawFirstVertexIndex == submeshB.rawFirstVertexIndex
				))
					return true;
			}

			return false;
#else
			// In normal immutable triangle use, immutableTriangles will be initially false, forcing the smartmesh to update the first time but never again after that, unless there was an immutableTriangles flag mismatch..
			if (a.immutableTriangles || b.immutableTriangles)
				return (a.immutableTriangles != b.immutableTriangles);

			return true;
#endif
		}
    }
}
