using UnityEngine;

public class MouseRetical : MonoBehaviour
{
    [SerializeField] LayerMask _recalLayer;

    public bool canCheck;
    private bool hasChecked;

    [SerializeField] float _startScale;
    [SerializeField] float _maxScale;

    [SerializeField] float _scaleSpeed;

    [SerializeField] Vector3 _originalDetectionScale;

    private float _elapsedTime;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;

    private void Start()
    {
        _originalDetectionScale = transform.localScale;
        _elapsedTime = 0f;

        if (audioSource != null && audioClip != null)
        {
            audioSource.clip = audioClip;
        }
    }

    /// <summary>
    /// Changes the size of the detection for a bigger range while recalling
    /// </summary>
    public void DetectionPulse()
    {
        _elapsedTime += Time.deltaTime * _scaleSpeed;

        float scale = Mathf.Lerp(_startScale, _maxScale, _elapsedTime);
        scale = Mathf.Min(scale, _maxScale);

        transform.localScale = _originalDetectionScale * scale;
        transform.localScale = new Vector3(transform.localScale.x, 0.25f, transform.localScale.z);
    }


    void Update()
    {
        if (canCheck)
        {
            if (!hasChecked)
            {
                hasChecked = true;
                audioSource.Play();
            }

            DetectionPulse();

            float detectionRadius = transform.localScale.x / 2;

            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, _recalLayer);
            foreach (Collider collider in colliders)
            {
                collider.GetComponent<RobotAI>().Recal();
            }
        }
        else
        {
            transform.localScale = _originalDetectionScale;
            _elapsedTime = 0f;

            if (hasChecked)
            {
                hasChecked = false;
                audioSource.Stop();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, transform.localScale.x / 2);
    }
}