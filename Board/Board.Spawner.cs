using MVE.SalExt;

namespace MVE;

public partial class Board : Node
{
    public enum LevelState
    {
        Invalid,
        OpeningWaiting,
        TravelToSelecting,
        Main,
        Finished,
        Lost
    }

    [Export] protected PackedScene cardScene = default!;
    [Export] protected PackedScene selectingUIScene = default!;
    [Export] protected PackedScene readySetScene = default!;
    [Export] protected AudioStream awoogaAudio = default!;
    [Export] protected Vector2 cameraStartPos;
    [Export] protected Vector2 cameraBoardPos;
    [Export] protected Vector2 cameraCardSelectingPos;
    protected AudioStreamPlayer awoogaAudioPlayer = default!;
    protected SceneTreeTimer? sceneTreeTimer;
    protected SelectingUI selectingUI = default!;
    protected StateMachine<LevelState> stateMachine = default!;

    protected Action<double> updater = default!;

    protected int[] rowWeights = new int[5];

    public int CurrentWave { get; protected set; }
    public LevelData LevelData { get; set; } = default!;
    public bool SpawnerBeginningReadyed { get; protected set; } = false;

    public void InitSpawner()
    {
        if (LevelData is null)
        {
            Game.Logger.LogError("LevelLoading", "No LevelData is assigned.");
        }
        awoogaAudioPlayer = SalAudioPool.GetPlayer(new(awoogaAudio, Bus: "Board"));

        stateMachine = new(LevelState.OpeningWaiting);
        stateMachine.RegisterState(LevelState.OpeningWaiting, LevelOpeningWaiting);
        stateMachine.RegisterState(LevelState.TravelToSelecting, LevelTravelToSelecting);
        stateMachine.RegisterState(LevelState.Main, LevelMain);
        stateMachine.RegisterState(LevelState.Finished);
        stateMachine.RegisterState(LevelState.Lost);
        stateMachine.EnterCurrent();
    }

    #region LevelCoroutine

    public async void LevelOpeningWaiting(LevelState pst)
    {
        CurrentWave = 0;
        boardUIManager.PlayHideAnimation();
        //开场等待
        await ToSignal(GetTree().CreateTimer(1d), SceneTreeTimer.SignalName.Timeout);

        stateMachine.State = LevelState.TravelToSelecting;
    }

    public async void LevelTravelToSelecting(LevelState pst)
    {
        //开场移动到最右边
        camera.Position = cameraStartPos;
        var cameraTween = camera.CreateTween();
        cameraTween.SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
        cameraTween.TweenProperty(camera, "position", cameraCardSelectingPos, 1d);
        await ToSignal(cameraTween, Tween.SignalName.Finished);

        //放置选卡ui
        var ui = selectingUIScene.Instantiate<SelectingUI>();
        ui.Modulate = ui.Modulate with { A = 0 };
        var finalCards = Game.Instance.SaveData.OwnedCards
            .Union(LevelData.Inventory.CardsAlwaysIncludes)
            .ToList();
        ui.CardsForSelecting = finalCards;
        layerOverlay.AddChild(ui);
        layerOverlay.MoveChild(ui, boardUIManager.GetIndex() - 1);
        ui.StartAppearAnimation();
        await ToSignal(ui, SelectingUI.SignalName.CardSelectingFinished);

        selectingUI = ui;
        stateMachine.State = LevelState.Main;
    }

    public async void LevelMain(LevelState pst)
    {
        if (pst is LevelState.TravelToSelecting)
        {
            var ui = selectingUI;
            //选卡结束, 制作选好的卡到boardUI的动画
            var cardTween = CreateTween()
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Sine)
                .SetParallel();
            var selectedCards = ui.SelectedCardsNode2D.GetChildren().OfType<CardForSelecting>();
            foreach (var c in selectedCards)
            {
                Vector2 targetPos = Extensions.SwitchTransform(ui, boardUIManager, c.Position) + boardUIManager.CardsLayoutStartPos;
                cardTween.TweenProperty(c, "position", targetPos, 0.5);
                c.ChangeDisabledState(true);
                c.Switch2DParent(boardUIManager);
            }

            //移动到版面
            var cameraTween2 = camera.CreateTween();
            cameraTween2.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
            cameraTween2.TweenProperty(camera, "position", cameraBoardPos, 1d);
            await ToSignal(cameraTween2, Tween.SignalName.Finished);

            ReadySetUI u = readySetScene.Instantiate<ReadySetUI>();
            u.Position = GetViewport().GetVisibleRect().GetCenter();
            boardUIManager.AddChild(u);
            await u.PlayAndFree();
            u.QueueFree();

            //销毁所有的ForSelecting的Card并生成并赋值真正的Card
            foreach (var c in selectedCards)
            {
                Vector2 pos = c.Position;
                var s = cardScene.Instantiate<Card>();
                s.Position = pos;
                s.WeaponPropertyId = c.WeaponPropertyId;
                c.Free();
                boardUIManager.AddChild(s);
            }
        }


        //显示除卡以外的其他ui
        boardUIManager.PlayDisplayAnimation();

        //切换ui显示模式(从LayerOverlay到LayerMain)
        boardUIManager.Switch2DParent(layerMain);
        layerMain.MoveChild(boardUIManager, -2);

        //刷怪开始
        updater = _ =>
        {
            var count = GetTree().GetNodesInGroup("Enemy").Count;
            if (count == 0 && SpawnerBeginningReadyed)
            {
                sceneTreeTimer!.TimeLeft = Math.Clamp(sceneTreeTimer!.TimeLeft, 0d, 0.75d);
            }
        };
        SpawnerBeginningReadyed = false;
        sceneTreeTimer = GetTree().CreateTimer(50d);
        await ToSignal(sceneTreeTimer, SceneTreeTimer.SignalName.Timeout);
        SpawnerBeginningReadyed = true;
        awoogaAudioPlayer.Play();
        NextWave();
        while (true)
        {
            sceneTreeTimer = GetTree().CreateTimer(Random.NextDouble(10, 14));
            await ToSignal(sceneTreeTimer, SceneTreeTimer.SignalName.Timeout);
            NextWave();
        }
    }

    #endregion

    public void UpdateSpawner(double delta)
        => updater?.Invoke(delta);

    public void NextWave()
    {
        CurrentWave += 1;

        int points = CurrentWave * LevelData.EnemiesSpawning.PointsAddFactor;

        SummonEnemiesBy(LevelData.EnemiesSpawning, points, PlaceEnemy);

        Enemy PlaceEnemy(in EnemyProperty property, int row)
            => Lawn.PlantingArea.PlaceEnemyAt(new Vector2I(Random.Next(10, 13), row), property);
    }

    public delegate Enemy EnemyPlacingHandler(in EnemyProperty property, int row);

    public void SummonEnemiesBy(EnemiesSpawningData spawningData, int points, EnemyPlacingHandler placingHandler)
    {
        var leastCost = spawningData.EnemyPool.Min(u => u.Cost);
        if (leastCost > points)
        {
            int row = Random.Next(0, 5);
            var prop = Game.Instance.EnemyProperties["zombie"];
            placingHandler(prop, row);
            var u = spawningData.EnemyPool.FirstOrDefault(u => u.Id == "zombie");
            rowWeights[row] -= u is not null ? u.Cost : 100;
            return;
        }

        while (points > 0)
        {
            var unit = spawningData.ChooseUnit(Random, points);
            if (unit is null) break;
            points -= unit.Cost;

            var curMax = rowWeights.Max();
            var allMax = rowWeights.Select((i, ind) => (i, ind)).Where(i => i.i == curMax);
            var allMaxList = allMax.ToList();
            (int i, int ind) result = Chooser<(int, int)>.ChooseFrom(Random, allMaxList);
            rowWeights[result.ind] -= unit.Cost;

            placingHandler(Game.Instance.EnemyProperties[unit.Id], result.ind);
        }
    }
}
