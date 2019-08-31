using System.Collections.Generic;

namespace AstroBot.Objects
{
    public class Flux
    {
        public static IDictionary<FluxType, FluxRange> FluxRangesLookup =>
            new Dictionary<FluxType, FluxRange>
            {
                [FluxType.V] = new FluxRange { From = "551nm", To = "88nm" },
                [FluxType.B] = new FluxRange { From = "445nm", To = "94nm" },
                [FluxType.R] = new FluxRange { From = "658nm", To = "138nm" },
                [FluxType.U] = new FluxRange { From = "365nm", To = "66nm" },
                [FluxType.I] = new FluxRange { From = "806nm", To = "149nm" },
                [FluxType.J] = new FluxRange { From = "1220nm", To = "213nm" },
                [FluxType.H] = new FluxRange { From = "1630nm", To = "307nm" },
                [FluxType.K] = new FluxRange { From = "2190nm", To = "390nm" },
            };

        public float Value { get; set; }

        public FluxType FluxType { get; set; }

        public class FluxRange
        {
            public string From { get; set; }
            public string To { get; set; }
        }
    }

    public enum FluxType
    {
        /// <summary>
        /// Visual (551nm - 88nm)
        /// </summary>
        V,

        /// <summary>
        /// Blue (445nm - 94nm)
        /// </summary>
        B,

        /// <summary>
        /// Ultraviolet (365nm - 66nm)
        /// </summary>
        U,

        /// <summary>
        /// Red (658nm - 138nm)
        /// </summary>
        R,

        /// <summary>
        /// Infrared (806nm - 149nm)
        /// </summary>
        I,

        /// <summary>
        /// 1220nm - 213nm
        /// </summary>
        J,

        /// <summary>
        /// 1630nm - 307nm
        /// </summary>
        H,

        /// <summary>
        /// 2190nm - 390nm
        /// </summary>
        K
    }
}