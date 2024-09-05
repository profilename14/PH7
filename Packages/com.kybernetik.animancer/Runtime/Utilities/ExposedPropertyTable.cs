// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

#if UNITY_PLAYABLE_DIRECTOR

using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>Sets a <see cref="PlayableDirector"/> as Animancer's <see cref="IExposedPropertyTable"/>.</summary>
    /// <remarks>
    /// This class allows Control Tracks to work properly when played in a <see cref="PlayableAssetState"/>.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/timeline#exposed-references">
    /// Exposed References</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ExposedPropertyTable
    /// 
    [AddComponentMenu(Strings.MenuPrefix + "Exposed Property Table")]
    [AnimancerHelpUrl(typeof(ExposedPropertyTable))]
    [DefaultExecutionOrder(-10000)]// Initialize before anything else might need to use the table.
    public class ExposedPropertyTable : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private PlayableDirector _Director;

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="OnValidate"/> and if no <see cref="PlayableDirector"/> was found it adds one.</summary>
        protected virtual void Reset()
        {
            OnValidate();

            if (_Director == null)
                _Director = gameObject.AddComponent<PlayableDirector>();

            _Director.enabled = false;
            _Director.playOnAwake = false;
        }

        /************************************************************************************************************************/

        /// <summary>Tries to automatically find any missing references.</summary>
        protected virtual void OnValidate()
        {
            gameObject.GetComponentInParentOrChildren(ref _Animancer);
            gameObject.GetComponentInParentOrChildren(ref _Director);
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="PlayableDirector"/> as Animancer's <see cref="IExposedPropertyTable"/>.</summary>
        protected virtual void Awake()
        {
            _Animancer.Graph.PlayableGraph.SetResolver(_Director);
        }

        /************************************************************************************************************************/
    }
}

#endif

