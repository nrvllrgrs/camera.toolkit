namespace UnityEngine.Rendering.Toolkit
{
	public class FlyCamera : MonoBehaviour
	{
		#region Fields

		[SerializeField]
		private Camera m_camera;

		[Header("Move Settings")]

		[SerializeField]
		private Space m_altitudeSpace = Space.Self;

		[Header("Look Settings")]

		[SerializeField]
		private Vector2 m_pitchLimits = new Vector2(-90f, 90f);

		[SerializeField]
		private Vector2 m_headingLimits = new Vector2(-180f, 180f);

		#endregion

		#region Properties

		public new Camera camera => m_camera;

		public float minPitch => m_pitchLimits.x;
		public float maxPitch => m_pitchLimits.y;

		public float minHeading => m_headingLimits.x;
		public float maxHeading => m_headingLimits.y;

		#endregion

		#region Methods

		public void Move(float rightDelta, float forwardDelta)
		{
			Move(new Vector2(rightDelta, forwardDelta));
		}

		public void Move(Vector2 v)
		{
			transform.Translate(v.x, 0f, v.y, Space.Self);
		}

		public void Look(float pitchDelta, float headingDelta)
		{
			transform.localRotation = Quaternion.Euler(
				Mathf.Clamp((transform.localEulerAngles.x + pitchDelta).WrapAngle(), minPitch, maxPitch),
				Mathf.Clamp((transform.localEulerAngles.y + headingDelta).WrapAngle(), minHeading, maxHeading),
				0f);
		}

		public void Look(Vector2 v)
		{
			Look(v.x, v.y);
		}

		public void Altitude(float upDelta)
		{
			Vector3 position;
			switch (m_altitudeSpace)
			{
				case Space.Self:
					position = transform.localPosition;
					position.y += upDelta;
					transform.localPosition = position;
					break;

				case Space.World:
					position = transform.position;
					position.y += upDelta;
					transform.position = position;
					break;
			}
		}

		#endregion
	}
}