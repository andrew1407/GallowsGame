using UnityEngine;

public class CouldsMovement : MonoBehaviour
{
    [Range(-1, 1)]
    [SerializeField] private int _direction;

    [SerializeField] private float _speed;

    private const int OUT_OF_BORDER = 14;

    private float _startPosition;

    private float _positionX
    {
        get => transform.position.x;
        set
        {
            Vector3 position = transform.position;
            position.x = value;
            transform.position = position;
        }
    }

    private void Start()
    {
        _startPosition = OUT_OF_BORDER * -_direction;
        _positionX = _startPosition;
    }

    private void Update()
    {
        _positionX += Time.deltaTime * _speed * _direction;
    }

    private void OnBecameInvisible()
    {
        lock (transform) _positionX = _startPosition;    
    }
}
