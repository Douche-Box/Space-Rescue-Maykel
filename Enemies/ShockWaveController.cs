using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class ShockWaveController : MonoBehaviour
{
    public bool canShockwave;

    [SerializeField] Collider _collider;

    [SerializeField] float _initialInnerRadius = 0f;
    [SerializeField] float _initialOuterRadius = 1f;

    [SerializeField] float _speed;

    [SerializeField] float _maxRadius = 10f;

    private float _currentInnerRadius;
    private float _currentOuterRadius;

    [SerializeField] float _pushBackForce;

    [SerializeField] VisualEffect _shockWaveEffect;

    private void Start()
    {
        _shockWaveEffect.enabled = false;
        _currentInnerRadius = _initialInnerRadius;
        _currentOuterRadius = _initialOuterRadius;
    }

    // Grows the shokwave overtime
    private void Update()
    {
        if (canShockwave)
        {
            _shockWaveEffect.enabled = true;
            _shockWaveEffect.Play();
            if (_currentOuterRadius < _maxRadius)
            {
                _currentInnerRadius += _speed * Time.deltaTime;
                _currentOuterRadius += _speed * Time.deltaTime;
            }
            else if (_currentOuterRadius >= _maxRadius)
            {
                canShockwave = false;

                _collider.enabled = false;

                _shockWaveEffect.Stop();
                _shockWaveEffect.enabled = false;

                transform.localScale = Vector3.one * _initialInnerRadius;

                _currentInnerRadius = _initialInnerRadius;
                _currentOuterRadius = _initialOuterRadius;
            }

            transform.localScale = new Vector3(_currentOuterRadius * 2, transform.localScale.y, _currentOuterRadius * 2);
        }
    }

    public void Shockwave()
    {
        canShockwave = true;

        _collider.enabled = true;
    }

    // Pushes back the prey when doing the shokwave
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Robot"))
        {
            Debug.Log("robot");

            float distanceFromCentre = Vector3.Distance(transform.position, other.transform.position);

            if (distanceFromCentre >= _currentInnerRadius)
            {
                Debug.Log("Shockwave");

                // Push the NavMeshAgent back
                NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    // Calculate the direction away from the shockwave center
                    Vector3 direction = (other.transform.position - transform.position).normalized;

                    // Calculate the new velocity for pushback
                    agent.velocity += direction * _pushBackForce;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _currentOuterRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _currentInnerRadius);
    }
}
