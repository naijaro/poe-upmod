using Patchwork.Attributes;

namespace UPMod
{
    [ModifiesType]
    public class mod_ConsumePotion : AI.Achievement.ConsumePotion
    {
        public int ConsumeAnimation
        {
            [ModifiesMember]
            get
            {
                // PATCH START:
                // RELATED BUGS: #6
                // NOTES:
                // - drinking potions occasionaly fizzles (unrelated to interruption); the workaround is to make drinking faster
                // - default potion consumption animation duration is 2.333333s (although if you will query this.m_animation.GetLengthOfCurrentAnim early on, the result will be 1.333333s)
                // - we will increase it's speed multiplier, in order to make the animation 1s long
                float animationDuration = 2.333333f; // this.m_animation.GetLengthOfCurrentAnim();
                SetSpeedMultiplier(animationDuration);
                // PATCH END

                return this.m_consumeAnimation;
            }
            [ModifiesMember]
            set
            {
                this.m_consumeAnimation = value;
            }
            // EDIT END
        }
    }
}