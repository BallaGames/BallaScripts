namespace Balla.Gameplay.Player
{
    public enum MovementState
    {
        None = 0,
        Walk = 1,
        Crouch = 2,
        Sprint = 4,
        Slide = 8,
        Air = 16,
        Ladder = 32,
        Mantle = 64,
        Special = 128,
    }
}