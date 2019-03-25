using UnityEngine;

/// <summary>
/// Player movement class, responsible for receiving the inputs from keyboard and using the move() method from the Character2DController.
/// </summary>
public class PlayerMovement : MonoBehaviour
{           
    public Character2DController m_PlayerController;

    private float m_PlayerHorizontalMove = 0f;
    private float m_PlayerVerticalMove = 0f;
    [SerializeField] private bool m_PlayerJump = false;
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

        if (Input.GetButton("Run"))
            m_PlayerRun = true;
        else
            m_PlayerRun = false;
    }

    void FixedUpdate()
    {
        m_PlayerController.Move(m_PlayerHorizontalMove, m_PlayerVerticalMove, m_PlayerJump, m_PlayerRun);

        m_PlayerJump = false;
    }
}
