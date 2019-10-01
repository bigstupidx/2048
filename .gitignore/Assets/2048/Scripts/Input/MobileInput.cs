using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MobileInput : MonoBehaviour, IGameInput, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private bool _isTouched;
    private float _maxTouchTime = 0.5f;
    private float _currentTouchTime;
    private bool _isTimeExpired;

    public float _minDragDistance = 125;

    private float x, y;

    private bool _isSlideToLeft;
    private bool _isSlideToRight;
    private bool _isSlideToUp;
    private bool _isSlideToDown;

    private void Update()
    {
        if (_isTouched && !_isTimeExpired)
        {
            _currentTouchTime += Time.deltaTime;
            _isTimeExpired |= _currentTouchTime > _maxTouchTime;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isTouched = true;
        _isTimeExpired = false;
        _currentTouchTime = 0;

        x = 0;
        y = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isTimeExpired)
        {
            x += eventData.delta.x;
            y += eventData.delta.y;

            if(Mathf.Abs(x) > Mathf.Abs(y))
            {
                if(x > 0)
                    _isSlideToRight |= x > _minDragDistance;
                else
                    _isSlideToLeft |= x < -_minDragDistance;
            }
            else
            {
                if (y > 0)
                    _isSlideToUp |= y > _minDragDistance;
                else
                    _isSlideToDown |= y < -_minDragDistance;
            }

            _isTimeExpired |= (_isSlideToLeft || _isSlideToRight || _isSlideToUp || _isSlideToDown);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isTouched = false;
    }

    public bool IsSlideToLeft()
    {
        if(_isSlideToLeft)
        {
            _isSlideToLeft = false;
            return true;
        }

        return false;
    }

    public bool IsSlideToRight()
    {
        if (_isSlideToRight)
        {
            _isSlideToRight = false;
            return true;
        }

        return false;
    }

    public bool IsSlideToUp()
    {
        if (_isSlideToUp)
        {
            _isSlideToUp = false;
            return true;
        }

        return false;
    }

    public bool IsSlideToDown()
    {
        if (_isSlideToDown)
        {
            _isSlideToDown = false;
            return true;
        }

        return false;
    }
}
