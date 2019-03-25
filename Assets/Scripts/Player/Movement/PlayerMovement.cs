using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Character2DController m_PlayerController;

    private float m_PlayerHorizontalMove = 0f;
    private float m_PlayerVerticalMove = 0f;
    private bool m_PlayerJump = false;
    private bool m_PlayerRun = false;


    // Update is called once per frame
    void Update()
    {
        m_PlayerHorizontalMove = Input.GetAxisRaw("Horizontal");
        m_PlayerVerticalMove = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            m_PlayerJump = true;
        }
    }

    void FixedUpdate()
    {
        m_PlayerController.Move(m_PlayerHorizontalMove * Time.fixedDeltaTime, m_PlayerVerticalMove * Time.fixedDeltaTime, m_PlayerJump, m_PlayerRun);

        m_PlayerJump = false;
    }
}
