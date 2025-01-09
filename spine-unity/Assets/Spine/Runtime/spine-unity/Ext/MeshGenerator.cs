using System;
using UnityEngine;

namespace Spine.Unity
{
    public partial class MeshGenerator
    {
        public void BuildPartsRenderMeshWithArrays (SkeletonRendererInstruction instruction, bool updateTriangles) {
			Settings settings = this.settings;
			bool canvasGroupTintBlack = settings.tintBlack && settings.canvasGroupCompatible;
			int totalVertexCount = instruction.rawVertexCount;

			// Add data to vertex buffers
			{
				if (totalVertexCount > vertexBuffer.Items.Length) { // Manual ExposedList.Resize()
					Array.Resize(ref vertexBuffer.Items, totalVertexCount);
					Array.Resize(ref uvBuffer.Items, totalVertexCount);
					Array.Resize(ref colorBuffer.Items, totalVertexCount);
				}
				vertexBuffer.Count = uvBuffer.Count = colorBuffer.Count = totalVertexCount;
			}

			// Populate Verts
			Color32 color = default(Color32);

			int vertexIndex = 0;
			float[] tempVerts = this.tempVerts;
			Vector2 bmin = this.meshBoundsMin;
			Vector2 bmax = this.meshBoundsMax;

			Vector3[] vbi = vertexBuffer.Items;
			Vector2[] ubi = uvBuffer.Items;
			Color32[] cbi = colorBuffer.Items;
			int lastSlotIndex = 0;

			// drawOrder[endSlot] is excluded
			for (int si = 0, n = instruction.submeshInstructions.Count; si < n; si++) {
				SubmeshInstruction submesh = instruction.submeshInstructions.Items[si];
				Skeleton skeleton = submesh.skeleton;
				Slot[] drawOrderItems = skeleton.DrawOrder.Items;
				float a = skeleton.A, r = skeleton.R, g = skeleton.G, b = skeleton.B;

				int endSlot = submesh.endSlot;
				int startSlot = submesh.startSlot;
				lastSlotIndex = endSlot;

				if (settings.tintBlack) {
					Vector2 rg, b2;
					int vi = vertexIndex;
					b2.y = 1f;

					PrepareOptionalUVBuffer(ref uv2, totalVertexCount);
					PrepareOptionalUVBuffer(ref uv3, totalVertexCount);

					Vector2[] uv2i = uv2.Items;
					Vector2[] uv3i = uv3.Items;

					for (int slotIndex = startSlot; slotIndex < endSlot; slotIndex++) {
						Slot slot = drawOrderItems[slotIndex];
						if (!slot.Bone.Active
#if SLOT_ALPHA_DISABLES_ATTACHMENT
							|| slot.A == 0f
#endif
							) continue;
						Attachment attachment = slot.Attachment;

						rg.x = slot.R2; //r
						rg.y = slot.G2; //g
						b2.x = slot.B2; //b
						b2.y = 1.0f;

						RegionAttachment regionAttachment = attachment as RegionAttachment;
						if (regionAttachment != null) {
							if (settings.pmaVertexColors) {
								float alpha = a * slot.A * regionAttachment.A;
								rg.x *= alpha;
								rg.y *= alpha;
								b2.x *= alpha;
								b2.y = slot.Data.BlendMode == BlendMode.Additive ? 0 : alpha;
							}
							uv2i[vi] = rg; uv2i[vi + 1] = rg; uv2i[vi + 2] = rg; uv2i[vi + 3] = rg;
							uv3i[vi] = b2; uv3i[vi + 1] = b2; uv3i[vi + 2] = b2; uv3i[vi + 3] = b2;
							vi += 4;
						} else { //} if (settings.renderMeshes) {
							MeshAttachment meshAttachment = attachment as MeshAttachment;
							if (meshAttachment != null) {
								if (settings.pmaVertexColors) {
									float alpha = a * slot.A * meshAttachment.A;
									rg.x *= alpha;
									rg.y *= alpha;
									b2.x *= alpha;
									b2.y = slot.Data.BlendMode == BlendMode.Additive ? 0 : alpha;
								}
								int verticesArrayLength = meshAttachment.WorldVerticesLength;
								for (int iii = 0; iii < verticesArrayLength; iii += 2) {
									uv2i[vi] = rg;
									uv3i[vi] = b2;
									vi++;
								}
							}
						}
					}
				}

				for (int slotIndex = startSlot; slotIndex < endSlot; slotIndex++) {
					Slot slot = drawOrderItems[slotIndex];
					if (!slot.Bone.Active
#if SLOT_ALPHA_DISABLES_ATTACHMENT
						|| slot.A == 0f
#endif
						) continue;
					Attachment attachment = slot.Attachment;
                    float z = 0;

					RegionAttachment regionAttachment = attachment as RegionAttachment;
					if (regionAttachment != null) {
						regionAttachment.ComputeWorldVertices(slot, tempVerts, 0);

						float x1 = tempVerts[RegionAttachment.BLX], y1 = tempVerts[RegionAttachment.BLY];
						float x2 = tempVerts[RegionAttachment.ULX], y2 = tempVerts[RegionAttachment.ULY];
						float x3 = tempVerts[RegionAttachment.URX], y3 = tempVerts[RegionAttachment.URY];
						float x4 = tempVerts[RegionAttachment.BRX], y4 = tempVerts[RegionAttachment.BRY];
						vbi[vertexIndex].x = x1; vbi[vertexIndex].y = y1; vbi[vertexIndex].z = z;
						vbi[vertexIndex + 1].x = x4; vbi[vertexIndex + 1].y = y4; vbi[vertexIndex + 1].z = z;
						vbi[vertexIndex + 2].x = x2; vbi[vertexIndex + 2].y = y2; vbi[vertexIndex + 2].z = z;
						vbi[vertexIndex + 3].x = x3; vbi[vertexIndex + 3].y = y3; vbi[vertexIndex + 3].z = z;

						if (settings.pmaVertexColors) {
							color.a = (byte)(a * slot.A * regionAttachment.A * 255);
							color.r = (byte)(r * slot.R * regionAttachment.R * color.a);
							color.g = (byte)(g * slot.G * regionAttachment.G * color.a);
							color.b = (byte)(b * slot.B * regionAttachment.B * color.a);
							if (canvasGroupTintBlack) color.a = 255;
							else if (slot.Data.BlendMode == BlendMode.Additive) color.a = 0;

						} else {
							color.a = (byte)(a * slot.A * regionAttachment.A * 255);
							color.r = (byte)(r * slot.R * regionAttachment.R * 255);
							color.g = (byte)(g * slot.G * regionAttachment.G * 255);
							color.b = (byte)(b * slot.B * regionAttachment.B * 255);
						}

						cbi[vertexIndex] = color; cbi[vertexIndex + 1] = color; cbi[vertexIndex + 2] = color; cbi[vertexIndex + 3] = color;

						float[] regionUVs = regionAttachment.UVs;
						ubi[vertexIndex].x = regionUVs[RegionAttachment.BLX]; ubi[vertexIndex].y = regionUVs[RegionAttachment.BLY];
						ubi[vertexIndex + 1].x = regionUVs[RegionAttachment.BRX]; ubi[vertexIndex + 1].y = regionUVs[RegionAttachment.BRY];
						ubi[vertexIndex + 2].x = regionUVs[RegionAttachment.ULX]; ubi[vertexIndex + 2].y = regionUVs[RegionAttachment.ULY];
						ubi[vertexIndex + 3].x = regionUVs[RegionAttachment.URX]; ubi[vertexIndex + 3].y = regionUVs[RegionAttachment.URY];

						if (x1 < bmin.x) bmin.x = x1; // Potential first attachment bounds initialization. Initial min should not block initial max. Same for Y below.
						if (x1 > bmax.x) bmax.x = x1;
						if (x2 < bmin.x) bmin.x = x2;
						else if (x2 > bmax.x) bmax.x = x2;
						if (x3 < bmin.x) bmin.x = x3;
						else if (x3 > bmax.x) bmax.x = x3;
						if (x4 < bmin.x) bmin.x = x4;
						else if (x4 > bmax.x) bmax.x = x4;

						if (y1 < bmin.y) bmin.y = y1;
						if (y1 > bmax.y) bmax.y = y1;
						if (y2 < bmin.y) bmin.y = y2;
						else if (y2 > bmax.y) bmax.y = y2;
						if (y3 < bmin.y) bmin.y = y3;
						else if (y3 > bmax.y) bmax.y = y3;
						if (y4 < bmin.y) bmin.y = y4;
						else if (y4 > bmax.y) bmax.y = y4;

						vertexIndex += 4;
					} else { //if (settings.renderMeshes) {
						MeshAttachment meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							int verticesArrayLength = meshAttachment.WorldVerticesLength;
							if (tempVerts.Length < verticesArrayLength) this.tempVerts = tempVerts = new float[verticesArrayLength];
							meshAttachment.ComputeWorldVertices(slot, tempVerts);

							if (settings.pmaVertexColors) {
								color.a = (byte)(a * slot.A * meshAttachment.A * 255);
								color.r = (byte)(r * slot.R * meshAttachment.R * color.a);
								color.g = (byte)(g * slot.G * meshAttachment.G * color.a);
								color.b = (byte)(b * slot.B * meshAttachment.B * color.a);
								if (canvasGroupTintBlack) color.a = 255;
								else if (slot.Data.BlendMode == BlendMode.Additive) color.a = 0;
							} else {
								color.a = (byte)(a * slot.A * meshAttachment.A * 255);
								color.r = (byte)(r * slot.R * meshAttachment.R * 255);
								color.g = (byte)(g * slot.G * meshAttachment.G * 255);
								color.b = (byte)(b * slot.B * meshAttachment.B * 255);
							}

							float[] attachmentUVs = meshAttachment.UVs;

							// Potential first attachment bounds initialization. See conditions in RegionAttachment logic.
							if (vertexIndex == 0) {
								// Initial min should not block initial max.
								// vi == vertexIndex does not always mean the bounds are fresh. It could be a submesh. Do not nuke old values by omitting the check.
								// Should know that this is the first attachment in the submesh. slotIndex == startSlot could be an empty slot.
								float fx = tempVerts[0], fy = tempVerts[1];
								if (fx < bmin.x) bmin.x = fx;
								if (fx > bmax.x) bmax.x = fx;
								if (fy < bmin.y) bmin.y = fy;
								if (fy > bmax.y) bmax.y = fy;
							}

							for (int iii = 0; iii < verticesArrayLength; iii += 2) {
								float x = tempVerts[iii], y = tempVerts[iii + 1];
								vbi[vertexIndex].x = x; vbi[vertexIndex].y = y; vbi[vertexIndex].z = z;
								cbi[vertexIndex] = color; ubi[vertexIndex].x = attachmentUVs[iii]; ubi[vertexIndex].y = attachmentUVs[iii + 1];

								if (x < bmin.x) bmin.x = x;
								else if (x > bmax.x) bmax.x = x;

								if (y < bmin.y) bmin.y = y;
								else if (y > bmax.y) bmax.y = y;

								vertexIndex++;
							}
						}
					}
				}
			}

			this.meshBoundsMin = bmin;
			this.meshBoundsMax = bmax;

			int submeshInstructionCount = instruction.submeshInstructions.Count;
			submeshes.Count = submeshInstructionCount;

			if (settings.useCustomSpacing)
			{
				if (submeshInstructionCount > 0)
				{
					SubmeshInstruction lastSubMeshInstruction = instruction.submeshInstructions.Items[submeshInstructionCount - 1];
                    Slot slot = lastSubMeshInstruction.skeleton.Slots.Items[lastSubMeshInstruction.endSlot - 1];
					this.meshBoundsThickness =  slot.zOrder * settings.zSpacing;
                }
			}
			else
			{
				this.meshBoundsThickness = lastSlotIndex * settings.zSpacing;
			}

			// Add triangles
			if (updateTriangles) {
				// Match submesh buffers count with submeshInstruction count.
				if (this.submeshes.Items.Length < submeshInstructionCount) {
					this.submeshes.Resize(submeshInstructionCount);
					for (int i = 0, n = submeshInstructionCount; i < n; i++) {
						ExposedList<int> submeshBuffer = this.submeshes.Items[i];
						if (submeshBuffer == null)
							this.submeshes.Items[i] = new ExposedList<int>();
						else
							submeshBuffer.Clear(false);
					}
				}

				SubmeshInstruction[] submeshInstructionsItems = instruction.submeshInstructions.Items; // This relies on the resize above.

				// Fill the buffers.
				int attachmentFirstVertex = 0;
				for (int smbi = 0; smbi < submeshInstructionCount; smbi++) {
					SubmeshInstruction submeshInstruction = submeshInstructionsItems[smbi];
					ExposedList<int> currentSubmeshBuffer = this.submeshes.Items[smbi];
					{ //submesh.Resize(submesh.rawTriangleCount);
						int newTriangleCount = submeshInstruction.rawTriangleCount;
						if (newTriangleCount > currentSubmeshBuffer.Items.Length)
							Array.Resize(ref currentSubmeshBuffer.Items, newTriangleCount);
						else if (newTriangleCount < currentSubmeshBuffer.Items.Length) {
							// Zero the extra.
							int[] sbi = currentSubmeshBuffer.Items;
							for (int ei = newTriangleCount, nn = sbi.Length; ei < nn; ei++)
								sbi[ei] = 0;
						}
						currentSubmeshBuffer.Count = newTriangleCount;
					}

					int[] tris = currentSubmeshBuffer.Items;
					int triangleIndex = 0;
					Skeleton skeleton = submeshInstruction.skeleton;
					Slot[] drawOrderItems = skeleton.DrawOrder.Items;
					for (int slotIndex = submeshInstruction.startSlot, endSlot = submeshInstruction.endSlot; slotIndex < endSlot; slotIndex++) {
						Slot slot = drawOrderItems[slotIndex];
						if (!slot.Bone.Active
#if SLOT_ALPHA_DISABLES_ATTACHMENT
							|| slot.A == 0f
#endif
							) continue;

						Attachment attachment = drawOrderItems[slotIndex].Attachment;
						if (attachment is RegionAttachment) {
							tris[triangleIndex] = attachmentFirstVertex;
							tris[triangleIndex + 1] = attachmentFirstVertex + 2;
							tris[triangleIndex + 2] = attachmentFirstVertex + 1;
							tris[triangleIndex + 3] = attachmentFirstVertex + 2;
							tris[triangleIndex + 4] = attachmentFirstVertex + 3;
							tris[triangleIndex + 5] = attachmentFirstVertex + 1;
							triangleIndex += 6;
							attachmentFirstVertex += 4;
							continue;
						}
						MeshAttachment meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							int[] attachmentTriangles = meshAttachment.Triangles;
							for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii++, triangleIndex++)
								tris[triangleIndex] = attachmentFirstVertex + attachmentTriangles[ii];
							attachmentFirstVertex += meshAttachment.WorldVerticesLength >> 1; // length/2;
						}
					}
				}
			}
		}

		public void BuildPartsRenderMeshWithArrays (SkeletonPartRendererInstruction instruction, bool updateTriangles) {
			Settings settings = this.settings;
			bool canvasGroupTintBlack = settings.tintBlack && settings.canvasGroupCompatible;
			int totalVertexCount = instruction.rawVertexCount;

			// Add data to vertex buffers
			{
				if (totalVertexCount > vertexBuffer.Items.Length) { // Manual ExposedList.Resize()
					Array.Resize(ref vertexBuffer.Items, totalVertexCount);
					Array.Resize(ref uvBuffer.Items, totalVertexCount);
					Array.Resize(ref colorBuffer.Items, totalVertexCount);
				}
				vertexBuffer.Count = uvBuffer.Count = colorBuffer.Count = totalVertexCount;
			}

			// Populate Verts
			Color32 color = default(Color32);

			int vertexIndex = 0;
			float[] tempVerts = this.tempVerts;
			Vector2 bmin = this.meshBoundsMin;
			Vector2 bmax = this.meshBoundsMax;

			Vector3[] vbi = vertexBuffer.Items;
			Vector2[] ubi = uvBuffer.Items;
			Color32[] cbi = colorBuffer.Items;
			int lastSlotIndex = 0;
            var submeshInstructions = instruction.skeletonRendererInstruction.submeshInstructions.Items;
            
			// drawOrder[endSlot] is excluded
			for (int si = instruction.startSubmesh ; si < instruction.endSubmesh; si++) {
				SubmeshInstruction submesh = submeshInstructions[si];
				Skeleton skeleton = submesh.skeleton;
				Slot[] drawOrderItems = skeleton.DrawOrder.Items;
				float a = skeleton.A, r = skeleton.R, g = skeleton.G, b = skeleton.B;

				int endSlot = submesh.endSlot;
				int startSlot = submesh.startSlot;
				lastSlotIndex = endSlot;

				if (settings.tintBlack) {
					Vector2 rg, b2;
					int vi = vertexIndex;
					b2.y = 1f;

					PrepareOptionalUVBuffer(ref uv2, totalVertexCount);
					PrepareOptionalUVBuffer(ref uv3, totalVertexCount);

					Vector2[] uv2i = uv2.Items;
					Vector2[] uv3i = uv3.Items;

					for (int slotIndex = startSlot; slotIndex < endSlot; slotIndex++) {
						Slot slot = drawOrderItems[slotIndex];
						if (!slot.Bone.Active
#if SLOT_ALPHA_DISABLES_ATTACHMENT
							|| slot.A == 0f
#endif
							) continue;
						Attachment attachment = slot.Attachment;

						rg.x = slot.R2; //r
						rg.y = slot.G2; //g
						b2.x = slot.B2; //b
						b2.y = 1.0f;

						RegionAttachment regionAttachment = attachment as RegionAttachment;
						if (regionAttachment != null) {
							if (settings.pmaVertexColors) {
								float alpha = a * slot.A * regionAttachment.A;
								rg.x *= alpha;
								rg.y *= alpha;
								b2.x *= alpha;
								b2.y = slot.Data.BlendMode == BlendMode.Additive ? 0 : alpha;
							}
							uv2i[vi] = rg; uv2i[vi + 1] = rg; uv2i[vi + 2] = rg; uv2i[vi + 3] = rg;
							uv3i[vi] = b2; uv3i[vi + 1] = b2; uv3i[vi + 2] = b2; uv3i[vi + 3] = b2;
							vi += 4;
						} else { //} if (settings.renderMeshes) {
							MeshAttachment meshAttachment = attachment as MeshAttachment;
							if (meshAttachment != null) {
								if (settings.pmaVertexColors) {
									float alpha = a * slot.A * meshAttachment.A;
									rg.x *= alpha;
									rg.y *= alpha;
									b2.x *= alpha;
									b2.y = slot.Data.BlendMode == BlendMode.Additive ? 0 : alpha;
								}
								int verticesArrayLength = meshAttachment.WorldVerticesLength;
								for (int iii = 0; iii < verticesArrayLength; iii += 2) {
									uv2i[vi] = rg;
									uv3i[vi] = b2;
									vi++;
								}
							}
						}
					}
				}

				for (int slotIndex = startSlot; slotIndex < endSlot; slotIndex++) {
					Slot slot = drawOrderItems[slotIndex];
					if (!slot.Bone.Active
#if SLOT_ALPHA_DISABLES_ATTACHMENT
						|| slot.A == 0f
#endif
						) continue;
					Attachment attachment = slot.Attachment;
                    float z = 0;

					RegionAttachment regionAttachment = attachment as RegionAttachment;
					if (regionAttachment != null) {
						regionAttachment.ComputeWorldVertices(slot, tempVerts, 0);

						float x1 = tempVerts[RegionAttachment.BLX], y1 = tempVerts[RegionAttachment.BLY];
						float x2 = tempVerts[RegionAttachment.ULX], y2 = tempVerts[RegionAttachment.ULY];
						float x3 = tempVerts[RegionAttachment.URX], y3 = tempVerts[RegionAttachment.URY];
						float x4 = tempVerts[RegionAttachment.BRX], y4 = tempVerts[RegionAttachment.BRY];
						vbi[vertexIndex].x = x1; vbi[vertexIndex].y = y1; vbi[vertexIndex].z = z;
						vbi[vertexIndex + 1].x = x4; vbi[vertexIndex + 1].y = y4; vbi[vertexIndex + 1].z = z;
						vbi[vertexIndex + 2].x = x2; vbi[vertexIndex + 2].y = y2; vbi[vertexIndex + 2].z = z;
						vbi[vertexIndex + 3].x = x3; vbi[vertexIndex + 3].y = y3; vbi[vertexIndex + 3].z = z;

						if (settings.pmaVertexColors) {
							color.a = (byte)(a * slot.A * regionAttachment.A * 255);
							color.r = (byte)(r * slot.R * regionAttachment.R * color.a);
							color.g = (byte)(g * slot.G * regionAttachment.G * color.a);
							color.b = (byte)(b * slot.B * regionAttachment.B * color.a);
							if (canvasGroupTintBlack) color.a = 255;
							else if (slot.Data.BlendMode == BlendMode.Additive) color.a = 0;

						} else {
							color.a = (byte)(a * slot.A * regionAttachment.A * 255);
							color.r = (byte)(r * slot.R * regionAttachment.R * 255);
							color.g = (byte)(g * slot.G * regionAttachment.G * 255);
							color.b = (byte)(b * slot.B * regionAttachment.B * 255);
						}

						cbi[vertexIndex] = color; cbi[vertexIndex + 1] = color; cbi[vertexIndex + 2] = color; cbi[vertexIndex + 3] = color;

						float[] regionUVs = regionAttachment.UVs;
						ubi[vertexIndex].x = regionUVs[RegionAttachment.BLX]; ubi[vertexIndex].y = regionUVs[RegionAttachment.BLY];
						ubi[vertexIndex + 1].x = regionUVs[RegionAttachment.BRX]; ubi[vertexIndex + 1].y = regionUVs[RegionAttachment.BRY];
						ubi[vertexIndex + 2].x = regionUVs[RegionAttachment.ULX]; ubi[vertexIndex + 2].y = regionUVs[RegionAttachment.ULY];
						ubi[vertexIndex + 3].x = regionUVs[RegionAttachment.URX]; ubi[vertexIndex + 3].y = regionUVs[RegionAttachment.URY];

						if (x1 < bmin.x) bmin.x = x1; // Potential first attachment bounds initialization. Initial min should not block initial max. Same for Y below.
						if (x1 > bmax.x) bmax.x = x1;
						if (x2 < bmin.x) bmin.x = x2;
						else if (x2 > bmax.x) bmax.x = x2;
						if (x3 < bmin.x) bmin.x = x3;
						else if (x3 > bmax.x) bmax.x = x3;
						if (x4 < bmin.x) bmin.x = x4;
						else if (x4 > bmax.x) bmax.x = x4;

						if (y1 < bmin.y) bmin.y = y1;
						if (y1 > bmax.y) bmax.y = y1;
						if (y2 < bmin.y) bmin.y = y2;
						else if (y2 > bmax.y) bmax.y = y2;
						if (y3 < bmin.y) bmin.y = y3;
						else if (y3 > bmax.y) bmax.y = y3;
						if (y4 < bmin.y) bmin.y = y4;
						else if (y4 > bmax.y) bmax.y = y4;

						vertexIndex += 4;
					} else { //if (settings.renderMeshes) {
						MeshAttachment meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							int verticesArrayLength = meshAttachment.WorldVerticesLength;
							if (tempVerts.Length < verticesArrayLength) this.tempVerts = tempVerts = new float[verticesArrayLength];
							meshAttachment.ComputeWorldVertices(slot, tempVerts);

							if (settings.pmaVertexColors) {
								color.a = (byte)(a * slot.A * meshAttachment.A * 255);
								color.r = (byte)(r * slot.R * meshAttachment.R * color.a);
								color.g = (byte)(g * slot.G * meshAttachment.G * color.a);
								color.b = (byte)(b * slot.B * meshAttachment.B * color.a);
								if (canvasGroupTintBlack) color.a = 255;
								else if (slot.Data.BlendMode == BlendMode.Additive) color.a = 0;
							} else {
								color.a = (byte)(a * slot.A * meshAttachment.A * 255);
								color.r = (byte)(r * slot.R * meshAttachment.R * 255);
								color.g = (byte)(g * slot.G * meshAttachment.G * 255);
								color.b = (byte)(b * slot.B * meshAttachment.B * 255);
							}

							float[] attachmentUVs = meshAttachment.UVs;

							// Potential first attachment bounds initialization. See conditions in RegionAttachment logic.
							if (vertexIndex == 0) {
								// Initial min should not block initial max.
								// vi == vertexIndex does not always mean the bounds are fresh. It could be a submesh. Do not nuke old values by omitting the check.
								// Should know that this is the first attachment in the submesh. slotIndex == startSlot could be an empty slot.
								float fx = tempVerts[0], fy = tempVerts[1];
								if (fx < bmin.x) bmin.x = fx;
								if (fx > bmax.x) bmax.x = fx;
								if (fy < bmin.y) bmin.y = fy;
								if (fy > bmax.y) bmax.y = fy;
							}

							for (int iii = 0; iii < verticesArrayLength; iii += 2) {
								float x = tempVerts[iii], y = tempVerts[iii + 1];
								vbi[vertexIndex].x = x; vbi[vertexIndex].y = y; vbi[vertexIndex].z = z;
								cbi[vertexIndex] = color; ubi[vertexIndex].x = attachmentUVs[iii]; ubi[vertexIndex].y = attachmentUVs[iii + 1];

								if (x < bmin.x) bmin.x = x;
								else if (x > bmax.x) bmax.x = x;

								if (y < bmin.y) bmin.y = y;
								else if (y > bmax.y) bmax.y = y;

								vertexIndex++;
							}
						}
					}
				}
			}

			this.meshBoundsMin = bmin;
			this.meshBoundsMax = bmax;

            int submeshInstructionCount = instruction.submeshCount;
			submeshes.Count = submeshInstructionCount;

			if (settings.useCustomSpacing)
			{
				if (submeshInstructionCount > 0)
				{
					SubmeshInstruction lastSubMeshInstruction = submeshInstructions[instruction.endSubmesh - 1];
                    Slot slot = lastSubMeshInstruction.skeleton.Slots.Items[lastSubMeshInstruction.endSlot - 1];
					this.meshBoundsThickness =  slot.zOrder * settings.zSpacing;
                }
			}
			else
			{
				this.meshBoundsThickness = lastSlotIndex * settings.zSpacing;
			}

			// Add triangles
			if (updateTriangles) {
				// Match submesh buffers count with submeshInstruction count.
				if (this.submeshes.Items.Length < submeshInstructionCount) {
					this.submeshes.Resize(submeshInstructionCount);
					for (int i = 0, n = submeshInstructionCount; i < n; i++) {
						ExposedList<int> submeshBuffer = this.submeshes.Items[i];
						if (submeshBuffer == null)
							this.submeshes.Items[i] = new ExposedList<int>();
						else
							submeshBuffer.Clear(false);
					}
				}
                
				// Fill the buffers.
				int attachmentFirstVertex = 0;
				for (int smbi = 0; smbi < instruction.submeshCount; smbi++) {
					SubmeshInstruction submeshInstruction = submeshInstructions[smbi + instruction.startSubmesh];
					ExposedList<int> currentSubmeshBuffer = this.submeshes.Items[smbi];
					{ //submesh.Resize(submesh.rawTriangleCount);
						int newTriangleCount = submeshInstruction.rawTriangleCount;
						if (newTriangleCount > currentSubmeshBuffer.Items.Length)
							Array.Resize(ref currentSubmeshBuffer.Items, newTriangleCount);
						else if (newTriangleCount < currentSubmeshBuffer.Items.Length) {
							// Zero the extra.
							int[] sbi = currentSubmeshBuffer.Items;
							for (int ei = newTriangleCount, nn = sbi.Length; ei < nn; ei++)
								sbi[ei] = 0;
						}
						currentSubmeshBuffer.Count = newTriangleCount;
					}

					int[] tris = currentSubmeshBuffer.Items;
					int triangleIndex = 0;
					Skeleton skeleton = submeshInstruction.skeleton;
					Slot[] drawOrderItems = skeleton.DrawOrder.Items;
					for (int slotIndex = submeshInstruction.startSlot, endSlot = submeshInstruction.endSlot; slotIndex < endSlot; slotIndex++) {
						Slot slot = drawOrderItems[slotIndex];
						if (!slot.Bone.Active
#if SLOT_ALPHA_DISABLES_ATTACHMENT
							|| slot.A == 0f
#endif
							) continue;

						Attachment attachment = drawOrderItems[slotIndex].Attachment;
						if (attachment is RegionAttachment) {
							tris[triangleIndex] = attachmentFirstVertex;
							tris[triangleIndex + 1] = attachmentFirstVertex + 2;
							tris[triangleIndex + 2] = attachmentFirstVertex + 1;
							tris[triangleIndex + 3] = attachmentFirstVertex + 2;
							tris[triangleIndex + 4] = attachmentFirstVertex + 3;
							tris[triangleIndex + 5] = attachmentFirstVertex + 1;
							triangleIndex += 6;
							attachmentFirstVertex += 4;
							continue;
						}
						MeshAttachment meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							int[] attachmentTriangles = meshAttachment.Triangles;
							for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii++, triangleIndex++)
								tris[triangleIndex] = attachmentFirstVertex + attachmentTriangles[ii];
							attachmentFirstVertex += meshAttachment.WorldVerticesLength >> 1; // length/2;
						}
					}
				}
			}
		}
    }
}
