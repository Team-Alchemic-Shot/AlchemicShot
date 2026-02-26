using UnityEngine;

public class GunLook : MonoBehaviour
{
	[SerializeField]
	private bool useLocalRotation = true;
	[SerializeField]
	private float followSpeed = 20f;
	[SerializeField]
	private float pitchOffset = 90f;
	[SerializeField]
	private bool followYaw = false;
	[SerializeField]
	private bool followPosition = true;
	[SerializeField]
	private Vector3 cameraLocalOffset = new(0.2f, -0.2f, 0.5f);
	[SerializeField]
	private Vector3 gunRotationOffset = Vector3.zero;

	private Camera mainCamera;

	private void Awake()
	{
		mainCamera = Camera.main;
	}

	private void LateUpdate()
	{
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
			if (mainCamera == null)
			{
				return;
			}
		}

		float pitch = mainCamera.transform.eulerAngles.x;
		if (pitch > 180f)
		{
			pitch -= 360f;
		}
		pitch += pitchOffset;

		float yaw = followYaw ? mainCamera.transform.eulerAngles.y : 0f;
		Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f) * Quaternion.Euler(gunRotationOffset);

		if (useLocalRotation)
		{
			transform.localRotation = Quaternion.Slerp(
				transform.localRotation,
				targetRotation,
				followSpeed * Time.deltaTime);
		}
		else
		{
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRotation,
				followSpeed * Time.deltaTime);
		}

		if (followPosition)
		{
			Vector3 targetPosition = mainCamera.transform.TransformPoint(cameraLocalOffset);
			transform.position = Vector3.Lerp(
				transform.position,
				targetPosition,
				followSpeed * Time.deltaTime);
		}
	}
}