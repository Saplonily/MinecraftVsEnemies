using NullLib.CommandLine;

namespace MVE;

public partial class Board : Node
{
    public class MiscCmd
    {
        protected readonly Board board;
        protected readonly Lawn lawn;

        public MiscCmd(Board board, Lawn lawn)
        {
            this.board = board;
            this.lawn = lawn;
        }

        [Command(CommandName = "place_weapon")]
        public void PlaceWeapon(int x, int y, int weaponId)
        {
            lawn.PlantingArea.TryPlantAt(new Vector2I(x, y), Game.Instance.WeaponProperties[weaponId], out _);
        }

        [Command(CommandName = "place_enemy")]
        public void PlaceEnemy(int x, int y, int enemyId)
        {
            lawn.PlantingArea.PlaceEnemyAt(new Vector2I(x, y), Game.Instance.EnemyProperties[enemyId]);
        }

        [Command(CommandName = "produce")]
        public void Produce()
        {
            foreach (var node in lawn.GetTree().GetNodesInGroup("Furnace"))
            {
                if (node is Furnace furnace)
                {
                    furnace.Produce();
                }
            }
        }

        [Command(CommandName = "set_redstone")]
        public void SetRedstone(double amount)
        {
            board.Bank.AddRedstone(amount - board.Bank.Redstone);
        }

        [Command(CommandName = "kill_all")]
        public void KillAll()
        {
            var enemies = board.GetTree().GetNodesInGroup("Enemy").Cast<Enemy>();
            foreach (var e in enemies)
            {
                e.Hp = 0d;
            }
        }
    }
}
