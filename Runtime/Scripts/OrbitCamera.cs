using UnityEngine.Events;

namespace UnityEngine.Rendering.Toolkit
{
	public class OrbitCamera : MonoBehaviour
	{
		#region Variables

		[SerializeField]
		private Camera m_camera;

		[SerializeField]
		private Transform m_target;

		[Header("Orbit Settings")]

		[SerializeField]
		private Vector2 m_pitchLimits = new Vector2(-90f, 90f);

		[SerializeField]
		private Vector2 m_headingLimits = new Vector2(-180f, 180f);

		[Header("Zoom Settings")]

		[SerializeField]
		private bool m_useDistanceZoom = true;

		[SerializeField]
		private Vector2 m_distanceLimits;

		[SerializeField]
		private Vector2 m_fovLimits;

		private float m_pitch = 0f, m_heading = 0f, m_roll = 0f;
		private float m_zoomDistance, m_zoomFov, m_initZoomFov;
		private bool m_zoomDirty;

		#endregion

		#region Events

		//public UnityEvent onPitchChanged = new UnityEvent();
		//public UnityEvent onHeadingChanged = new UnityEvent();
		public UnityEvent onZoomDistanceChanged = new UnityEvent();
		public UnityEvent onZoomFovChanged = new UnityEvent();
		public UnityEvent onZoomChanged = new UnityEvent();

		#endregion

		#region Properties

		public new Camera camera => m_camera;
		public Transform target
		{
			get => m_target;
			set
			{
				m_target = value;
				Evaluate();
			}
		}

		public float pitch => m_pitch;
		public float heading => m_heading;
		public float roll { get => m_roll; set => m_roll = value; }

		public float minPitch => m_pitchLimits.x;
		public float maxPitch => m_pitchLimits.y;

		public float minDistance => m_distanceLimits.x;
		public float maxDistance => m_distanceLimits.y;

		public float minFov => m_fovLimits.x;
		public float maxFov => m_fovLimits.y;

		public float zoomDistance
		{
			get { return m_zoomDistance; }
			set
			{
				if (!m_useDistanceZoom)
					return;

				value = Mathf.Clamp(value, minDistance, maxDistance);
				if (zoomDistance == value && !m_zoomDirty)
					return;

				m_zoomDistance = value;
				UpdateOrthographicSize();
				Evaluate();

				onZoomDistanceChanged.Invoke();
				onZoomChanged.Invoke();
				m_zoomDirty = false;
			}
		}

		public float zoomFov
		{
			get { return m_zoomFov; }
			set
			{
				if (m_useDistanceZoom)
					return;

				if (camera.usePhysicalProperties)
				{
					value = Mathf.Clamp(value, minFov, maxFov);
					if (zoomFov == value && !m_zoomDirty)
						return;

					camera.focalLength = value;
				}
				else
				{
					value = Mathf.Clamp(value, maxFov, minFov);
					if (zoomFov == value && !m_zoomDirty)
						return;

					camera.fieldOfView = value;
				}

				m_zoomFov = value;
				UpdateOrthographicSize();
				Evaluate();

				onZoomFovChanged.Invoke();
				onZoomChanged.Invoke();
				m_zoomDirty = false;
			}
		}

		public float zoomPercent
		{
			get
			{
				if (m_useDistanceZoom)
				{
					return UnityUtil.GetPercent(zoomDistance, minDistance, maxDistance);
				}
				else
				{
					return camera.usePhysicalProperties
						? UnityUtil.GetPercent(camera.focalLength, minFov, maxFov)
						: UnityUtil.GetPercent(camera.fieldOfView, minFov, maxFov);
				}
			}
			set
			{
				if (m_useDistanceZoom)
				{
					zoomDistance = UnityUtil.Remap01(value, minDistance, maxDistance);
				}
				else
				{
					zoomFov = camera.usePhysicalProperties
						? UnityUtil.Remap01(value, minFov, maxFov)
						: UnityUtil.Remap01(value, minFov, maxFov);
				}

				Evaluate();
			}
		}

		#endregion

		private void Awake()
		{
			m_pitch = transform.eulerAngles.x;
			m_heading = transform.eulerAngles.y;

			if (target != null)
			{
				float distance = Vector3.Distance(transform.position, target.transform.position);
				if (!m_useDistanceZoom)
				{
					distance = Mathf.Clamp(distance, minDistance, maxDistance);
				}

				// Don't do this silently, let all listeners know
				m_zoomDistance = zoomDistance = distance;

				// Set FOV after after distance is initialized, required
				m_zoomFov = zoomFov = camera.usePhysicalProperties ? camera.focalLength : camera.fieldOfView;
			}
		}

		private void UpdateOrthographicSize()
		{
			if (camera.orthographic && !m_useDistanceZoom)
			{
				camera.orthographicSize = zoomDistance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
			}
		}

		public void ModifyPitchAndHeading(float pitchDelta, float headingDelta)
		{
			SetPitchAndHeading(pitch + pitchDelta, heading + headingDelta);
		}

		public void SetPitchAndHeading(float pitch, float heading)
		{
			m_pitch = Mathf.Clamp(pitch.WrapAngle(), m_pitchLimits.x, m_pitchLimits.y);
			m_heading = Mathf.Clamp(heading.WrapAngle(), m_headingLimits.x, m_headingLimits.y);
			Evaluate();
		}

		private void ModifyZoom(float delta)
		{
			if (m_useDistanceZoom)
			{
				zoomDistance += delta;
			}
			else
			{
				zoomFov += delta;
			}
		}

		private void SetZoom(float value)
		{
			if (m_useDistanceZoom)
			{
				zoomDistance = value;
			}
			else
			{
				zoomFov = value;
			}
		}

		private void Evaluate()
		{
			if (m_target == null)
				return;

			Quaternion rotation = Quaternion.Euler(m_pitch, m_heading, 0f) * Quaternion.Euler(0f, 0f, m_roll);
			transform.SetPositionAndRotation(
				rotation * (Vector3.back * zoomDistance) + target.position,
				rotation);
		}
	}
}