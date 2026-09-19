using Game;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Tools;
using Unity.Entities;
using Unity.Mathematics;

namespace TrafficLightIntoKorea {
// UI/tool phases only update this mailbox. Entity mutations run before object
// SearchSystem in Modification5, so Created survives until the first tree Add.
public partial class ManualSignalPreviewSystem : GameSystemBase {
    private Entity _preview, _source, _owner, _prefab;
    private Transform _requestedTransform;
    private bool _wanted, _visible, _replace, _loggedMove;

    internal void Begin(Entity owner, Entity source, Entity prefab, quaternion rotation) {
        _owner = owner; _source = source; _prefab = prefab;
        _requestedTransform = new Transform { m_Rotation = rotation };
        _wanted = true; _visible = false; _replace = true; _loggedMove = false;
        RuntimeDiagnostics.Event("Manual preview requested source=" + source + " prefab=" + prefab);
    }
    internal void Move(float3 point, bool visible) {
        _visible = visible && math.all(math.isfinite(point));
        if (_visible) _requestedTransform.m_Position = point;
    }
    internal void Cancel() { _wanted = false; _visible = false; }

    private bool Live(Entity entity) => EntityManager.Exists(entity)
        && !EntityManager.HasComponent<Deleted>(entity)
        && !EntityManager.HasComponent<KoreanRetired>(entity);

    private void Retire() {
        if (EntityManager.Exists(_preview)) {
            RuntimeDiagnostics.Event("Manual preview retire entity=" + _preview);
            World.GetOrCreateSystemManaged<KoreanVisualLifecycleSystem>().Retire(_preview);
        }
        _preview = Entity.Null;
    }

    // Called before the existing lifecycle save guard, which marks retired
    // objects Temp during serialization. It must also discard unconsumed requests.
    internal void BeforeSave() { Cancel(); Retire(); }
    protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode) {
        Cancel(); _preview = _source = _owner = _prefab = Entity.Null; _replace = false;
        base.OnGamePreload(purpose, mode);
    }
    protected override void OnUpdate() {
        if (!_wanted || !Live(_source) || !Live(_owner)
            || !KoreanJunctionSystem.Selected(EntityManager, _owner)
            || !Live(_prefab) || !EntityManager.HasComponent<ObjectData>(_prefab)) {
            Cancel(); Retire(); return;
        }
        if (_replace) { Retire(); _replace = false; }
        if (!_visible) {
            if (Live(_preview) && !EntityManager.HasComponent<Hidden>(_preview)) {
                EntityManager.AddComponent<Hidden>(_preview);
                MarkBatches();
            }
            return;
        }
        if (!Live(_preview)) {
            var archetype = EntityManager.GetComponentData<ObjectData>(_prefab).m_Archetype;
            if (!archetype.Valid) { Cancel(); return; }
            _preview = EntityManager.CreateEntity(archetype);
            EntityManager.SetComponentData(_preview, new PrefabRef { m_Prefab = _prefab });
            EntityManager.SetComponentData(_preview, _requestedTransform);
            EntityManager.AddComponent<KoreanSignalClone>(_preview);
            EntityManager.AddComponent<FarSignal>(_preview);
            EntityManager.AddComponent<Highlighted>(_preview);
            EntityManager.AddComponentData(_preview, new Owner { m_Owner = _owner });
            if (!EntityManager.HasComponent<Created>(_preview)) EntityManager.AddComponent<Created>(_preview);
            if (!EntityManager.HasComponent<Updated>(_preview)) EntityManager.AddComponent<Updated>(_preview);
            RuntimeDiagnostics.Event("Manual preview created before SearchSystem entity=" + _preview
                + " source=" + _source + " position=" + _requestedTransform.m_Position);
        } else {
            var previous = EntityManager.GetComponentData<Transform>(_preview);
            if (!previous.m_Position.Equals(_requestedTransform.m_Position)
                || !previous.m_Rotation.Equals(_requestedTransform.m_Rotation)) {
                EntityManager.SetComponentData(_preview, _requestedTransform);
                if (!EntityManager.HasComponent<Updated>(_preview)) EntityManager.AddComponent<Updated>(_preview);
                if (!_loggedMove) {
                    _loggedMove = true;
                    RuntimeDiagnostics.Event("Manual preview first move entity=" + _preview);
                }
            }
        }
        if (EntityManager.HasComponent<Hidden>(_preview)) {
            EntityManager.RemoveComponent<Hidden>(_preview); MarkBatches();
        }
        SignalRenderReadiness.Protect(EntityManager, _preview);
        if (EntityManager.HasComponent<TrafficLight>(_preview) && EntityManager.HasComponent<TrafficLight>(_source))
            EntityManager.SetComponentData(_preview, EntityManager.GetComponentData<TrafficLight>(_source));
    }
    private void MarkBatches() {
        if (!EntityManager.HasComponent<BatchesUpdated>(_preview)) EntityManager.AddComponent<BatchesUpdated>(_preview);
    }
}
}
