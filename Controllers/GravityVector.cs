namespace ThunderPulse.Controllers
{
    public enum GravityVector : byte
    {
        /// <summary>
        /// гравитационное ускорение по -Y
        /// </summary>
        StandartMenusY = 0,
        /// <summary>
        /// вектор гравитации перпендикулярен поверхности земли
        /// </summary>
        NormalToGroundSurface = 1,
        /// <summary>
        /// гравитации нет, есть только инерция в космосе
        /// </summary>
        CosmicInertia = 2,
    }
}
