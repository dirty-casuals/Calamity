using UnityEngine;
using psai.net;

public abstract class PsaiColliderBasedTrigger : PsaiSynchronizedTrigger
{
    [SerializeField()]
    private Collider playerCollider;
    public Collider PlayerCollider
    {
        get
        {
            return playerCollider;
        }

        set
        {
            playerCollider = value;
        }
    }

    [SerializeField()]
    private Collider localCollider;

    public Collider LocalCollider
    {
        get
        {
            return localCollider;
        }

        set
        {
            localCollider = value;
        }
    }

    private void TryToAutoAssignPlayerCollider()
    {
        if (playerCollider == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
            {
                string[] playerStrings = { "Player", "player", "PLAYER" };
                foreach (string s in playerStrings)
                {
                    playerObject = GameObject.Find(s);
                    if (playerObject != null)
                        break;
                }
            }

            if (playerObject != null)
            {
                playerCollider = playerObject.GetComponent<Collider>();
                if (playerCollider == null)
                {
                    playerObject.GetComponentInChildren<Collider>();
                }
            }

            #if !(PSAI_NOLOG)
            {
                if (playerCollider == null)
                {
                    Debug.LogError(string.Format("No Player Collider could be found for component {0}. Please assign the 'Player' tag to your player object, or assign the collider manually.", this.ToString()));
                }
                else
                {
                    if (PsaiCoreManager.Instance.logTriggerScripts)
                    {
                        Debug.Log(string.Format("successfully auto-assigned Player Collider in component {0}", this.ToString()));
                    }
                }
            }
            #endif
        }
    }

    private void TryToAutoAssignLocalCollider()
    {
        if (localCollider == null)
        {
            this.GetComponent<Collider>();
        }

        #if !(PSAI_NOLOG)
        {
            if (localCollider == null)
            {
                Debug.LogWarning(string.Format("No local Collider could be found for psai Trigger component: {0}. Please assign manually. ", this.ToString()));
            }
            else
            {
                if (PsaiCoreManager.Instance.logTriggerScripts)
                {
                    Debug.Log(string.Format("successfully auto-assigned Local Collider in component {0}", this.ToString()));
                }
            }
        }
        #endif
    }



    void Start()
    {
        if (PlayerCollider == null)
        {
            TryToAutoAssignPlayerCollider();
        }

        if (LocalCollider == null)
        {
            TryToAutoAssignLocalCollider();
        }
    }


}