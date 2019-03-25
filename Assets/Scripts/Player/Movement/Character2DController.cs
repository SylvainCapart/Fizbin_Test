using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Character2DController : MonoBehaviour
{
    private Rigidbody2D m_PlayerRigidbody;
    private Vector2 m_RefPlayerVelocity;
    [SerializeField] [Range(0f, 1f)] private float m_PlayerMovemementSmoothing = 0.1f;
    [SerializeField] private float m_PlayerSpeed = 70f;

    private bool m_FacingRight = true;

    private void Start()
    {
        m_PlayerRigidbody = GetComponent<Rigidbody2D>();
    }

    public void Move(float _PlayerHorizontalMove, float _PlayerVerticalMove, bool _PlayerJump, bool _PlayerRun)
    {
        Vector2 targetSpeed = new Vector2(_PlayerHorizontalMove * m_PlayerSpeed, _PlayerVerticalMove * m_PlayerSpeed);

        m_PlayerRigidbody.velocity = Vector2.SmoothDamp(m_PlayerRigidbody.velocity, targetSpeed, ref m_RefPlayerVelocity, m_PlayerMovemementSmoothing);

        if (m_FacingRight && _PlayerHorizontalMove < 0f)
        {
            FlipScale();
        }
        else if (!m_FacingRight && _PlayerHorizontalMove > 0f)
        {
            FlipScale();
        }
    }

    private void FlipScale()
    {
        m_FacingRight = !m_FacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

    }

}
