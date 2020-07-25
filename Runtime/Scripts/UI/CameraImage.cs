using UnityEngine.UI;

namespace UnityEngine.Rendering.Toolkit.UI
{
	[RequireComponent(typeof(RawImage))]
	public class CameraImage : MonoBehaviour
    {
		#region Fields

		[SerializeField]
		private Camera m_camera;

		[SerializeField]
		private string m_cameraTag;

		private RawImage m_rawImage;
		private RenderTexture m_targetTexture;

		#endregion

		#region Properties

		public new Camera camera
		{
			get
			{
				if (m_camera == null)
				{
					if (!string.IsNullOrWhiteSpace(m_cameraTag))
					{
						m_camera = GameObject.FindGameObjectWithTag(m_cameraTag)?.GetComponent<Camera>() ?? CameraUtil.main;
					}
					else
					{
						m_camera = CameraUtil.main;
					}
				}
				return m_camera;
			}
		}

		public RawImage rawImage => this.GetComponent(ref m_rawImage);

		#endregion

		#region Methods

		private void Start()
		{
			UpdateRenderTexture();
		}

		public void UpdateRenderTexture()
		{
			if (m_targetTexture != null)
			{
				m_targetTexture.Release();
			}

			// Need ratio of RawImage to ensure RenderTexture is not squashed / stretched
			float ratio = rawImage.rectTransform.rect.width / rawImage.rectTransform.rect.height;

			// Create RenderTexture with same dimensions as RawImage
			m_targetTexture = new RenderTexture((int)(Screen.height * ratio), Screen.height, 24);

			// Link RenderTexture to camera and RawImage
			camera.targetTexture = m_targetTexture;
			rawImage.texture = m_targetTexture as Texture;
		}

		#endregion
	}
}