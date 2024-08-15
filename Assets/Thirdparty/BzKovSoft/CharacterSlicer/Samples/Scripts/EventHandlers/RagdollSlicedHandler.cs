using System.Collections;
using BzKovSoft.ObjectSlicer;
using BzKovSoft.ObjectSlicer.EventHandlers;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.CharacterSlicer.Samples.EventHandlers
{
	/// <summary>
	/// Handler that will convert the character into the ragdoll if slice was successful
	/// </summary>
	[DisallowMultipleComponent]
	public class RagdollSlicedHandler : MonoBehaviour, IBzObjectSlicedEvent, IDeadable
	{
#pragma warning disable 0649
		[SerializeField]
		GameObject _bloodPrefub;
		[SerializeField]
		Vector3 _prefubDirection;
		[SerializeField]
		bool _convertToRagdoll = true;
		[SerializeField]
		bool _alignPrefSize = false;
#pragma warning restore 0649

		public bool IsDead { get; private set; }

		public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
		{
			return true;
		}

		public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
		{
			if (original.GetComponent<RagdollSlicedHandler>().IsDead)
			{
				return;
			}

			// add blood
			Profiler.BeginSample("AddBlood");
			AddBlood(result);
			Profiler.EndSample();

			Animator animator = this.GetComponent<Animator>();
			Vector3 velocityContinue = animator.velocity;
			Vector3 angularVelocityContinue = animator.angularVelocity;

			foreach (var resultObject in result.resultObjects)
			{
				var resultSlicer = resultObject.gameObject.GetComponent<RagdollSlicedHandler>();

				// convert to ragdoll
				if (_convertToRagdoll & !resultSlicer.IsDead)
				{
					Profiler.BeginSample("ConvertToRagdoll");
					ConvertToRagdoll(resultObject.gameObject, velocityContinue, angularVelocityContinue);
					Profiler.EndSample();
				}

				resultSlicer.IsDead = true;
			}
		}

		private void AddBlood(BzSliceTryResult result)
		{

			for (int i = 0; i < result.resultObjects.Length; i++)
			{
				var resultObject = result.resultObjects[i];

				foreach (var mesh in resultObject.meshes)
				{
					if (mesh.sliceType == SliceType.Slice)
					{
						for (int j = 0; j < mesh.sliceEdges.Length; j++)
						{
							var meshData = mesh.sliceEdges[j];
							SetBleedingObjects(meshData, mesh.renderer);
						}
					}
				}
			}
		}

		private void SetBleedingObjects(BzSliceEdgeResult edge, Renderer renderer)
		{
			if (_bloodPrefub == null)
			{
				return;
			}

			var meshRenderer = renderer as MeshRenderer;
			var skinnedRenderer = renderer as SkinnedMeshRenderer;

			GameObject blood = null;

			if (meshRenderer != null)
			{
				// add blood object
				Vector3 position = AVG(edge.vertices);
				Vector3 direction = AVG(edge.normals).normalized;
				var rotation = Quaternion.FromToRotation(_prefubDirection, direction);
				blood = Instantiate(_bloodPrefub, renderer.gameObject.transform);

				blood.transform.localPosition = position;
				blood.transform.localRotation = rotation;
			}
			else if (skinnedRenderer != null)
			{
				var bones = skinnedRenderer.bones;
				float[] weightSums = new float[bones.Length];
				for (int i = 0; i < edge.boneWeights.Length; i++)
				{
					var w = edge.boneWeights[i];
					weightSums[w.boneIndex0] += w.weight0;
					weightSums[w.boneIndex1] += w.weight1;
					weightSums[w.boneIndex2] += w.weight2;
					weightSums[w.boneIndex3] += w.weight3;
				}

				// detect most weightful bone for this PolyMeshData
				int maxIndex = 0;
				for (int i = 0; i < weightSums.Length; i++)
				{
					float maxValue = weightSums[maxIndex];
					float current = weightSums[i];

					if (current > maxValue)
						maxIndex = i;
				}
				Transform bone = bones[maxIndex];

				// add blood object to the bone
				Vector3 position = AVG(edge.vertices);
				Vector3 normal = AVG(edge.normals).normalized;
				var rotation = Quaternion.FromToRotation(_prefubDirection, normal);

				var m = skinnedRenderer.sharedMesh.bindposes[maxIndex];
				position = m.MultiplyPoint3x4(position);

				blood = Instantiate(_bloodPrefub, bone);
				blood.transform.localPosition = position;
				blood.transform.localRotation = rotation;
			}

			if (_alignPrefSize)
			{
				var parentScale = blood.transform.parent.lossyScale;
				var newScale = new Vector3(
					1f / parentScale.x,
					1f / parentScale.y,
					1f / parentScale.z);

				blood.transform.localScale = Vector3.Scale(newScale, blood.transform.localScale);
			}
		}

		private static Vector3 AVG(Vector3[] vertices)
		{
			Vector3 result = Vector3.zero;

			for (int i = 0; i < vertices.Length; i++)
			{
				result += vertices[i];
			}

			return result / vertices.Length;
		}

		private void ConvertToRagdoll(GameObject go, Vector3 velocityContinue, Vector3 angularVelocityContinue)
		{
			Profiler.BeginSample("ConvertToRagdoll");
			// if your player is dead, you do not need animator or collision collider
			Animator animator = go.GetComponent<Animator>();
			Collider triggerCollider = go.GetComponent<Collider>();

			UnityEngine.Object.Destroy(animator);
			UnityEngine.Object.Destroy(triggerCollider);

			var collidersArr = go.GetComponentsInChildren<Collider>();
			for (int i = 0; i < collidersArr.Length; i++)
			{
				var collider = collidersArr[i];
				if (collider == triggerCollider)
					continue;

				collider.isTrigger = false;
			}

			// set rigid bodies as non kinematic
			var rigidsArr = go.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < rigidsArr.Length; i++)
			{
				var rigid = rigidsArr[i];
				rigid.isKinematic = false;
			}

			SetVelocity(go, velocityContinue, angularVelocityContinue);
			StartCoroutine(SmoothDepenetration(go));

			Profiler.EndSample();
		}

		private static void SetVelocity(GameObject go, Vector3 velocityContinue, Vector3 angularVelocityContinue)
		{
			var rigids = go.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];

				rigid.velocity = velocityContinue;
				rigid.angularVelocity = angularVelocityContinue;
			}
		}

		private static IEnumerator SmoothDepenetration(GameObject go)
		{
			var rigids = go.GetComponentsInChildren<Rigidbody>();
			var maxVelocitys = new float[rigids.Length];
			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];
				maxVelocitys[i] = rigid.maxDepenetrationVelocity;
				rigid.maxDepenetrationVelocity = 0.1f;
			}

			yield return new WaitForSeconds(1);

			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];
				if (rigid == null)
					continue;

				float maxVel = maxVelocitys[i];
				rigid.maxDepenetrationVelocity = maxVel;
			}
		}
	}
}
