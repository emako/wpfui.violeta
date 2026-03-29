using System.Windows;

namespace iNKORE.UI.WPF.Modern.Common
{
    /// <summary>
    /// Represents information about the visual states of the elements that represent
    /// a rating.
    /// </summary>
    public class RatingItemInfo : Freezable
    {
        /// <summary>
        /// Initializes a new instance of the RatingItemInfo class.
        /// </summary>
        public RatingItemInfo()
        {
        }

        protected override Freezable CreateInstanceCore()
        {
            return new RatingItemInfo();
        }
    }
}
