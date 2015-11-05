using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Configuration;
using UCS.PacketProcessing;
using UCS.Core;
using UCS.GameFiles;
using Newtonsoft.Json;

namespace UCS.Logic
{
    class ResourceProductionComponent : Component
    {
        private const int m_vType = 0x01AB3F00;

        private DateTime m_vTimeSinceLastChange;
        private ResourceData m_vProductionResourceData;
        private float m_vCurrentResources;
        private List<int> m_vResourcesPerHour;
        private List<int> m_vMaxResources;

        public ResourceProductionComponent(ConstructionItem ci, Level level) : base(ci)
        {
            this.m_vTimeSinceLastChange = level.GetTime();
            m_vCurrentResources = 0;
            this.m_vProductionResourceData = ObjectManager.DataTables.GetResourceByName(((BuildingData)ci.GetData()).ProducesResource);
            this.m_vResourcesPerHour = ((BuildingData)ci.GetData()).ResourcePerHour;
            this.m_vMaxResources = ((BuildingData)ci.GetData()).ResourceMax;
        }

        public void CollectResources()
        {
            if (this.m_vCurrentResources >= 10)
            {
                ClientAvatar ca = GetParent().GetLevel().GetPlayerAvatar();

                ca.CommodityCountChangeHelper(0, this.m_vProductionResourceData, (int)this.m_vCurrentResources);
            }
        }

        public override void Tick()
        {
            ConstructionItem ci = (ConstructionItem)GetParent();
            Level level = GetParent().GetLevel();
            float deltaTime = (float)((level.GetTime() - this.m_vTimeSinceLastChange).TotalMilliseconds * 1000);//Time since last tick
            if (m_vCurrentResources < this.m_vMaxResources[ci.UpgradeLevel]){
                ClientAvatar ca = level.GetPlayerAvatar();
                this.m_vCurrentResources += ((this.m_vResourcesPerHour[ci.UpgradeLevel] / (60f * 60f)) * float.Parse(ConfigurationManager.AppSettings["ResourceMultiplier"]) *(deltaTime));

                this.m_vCurrentResources = Math.Min(Math.Max(this.m_vCurrentResources, 0), this.m_vMaxResources[ci.UpgradeLevel]);
                this.m_vTimeSinceLastChange = level.GetTime();
            }
        }

        public override int Type
        {
            get { return 5; }
        }
    }
}
