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
using Newtonsoft.Json.Linq;

namespace UCS.Logic
{
    class ResourceProductionComponent : Component
    {

        public override int Type
        {
            get { return 5; }
        }

        private const int m_vType = 0x01AB3F00;

        private DateTime m_vTimeSinceLastClick;
        private ResourceData m_vProductionResourceData;
        private List<int> m_vResourcesPerHour;
        private List<int> m_vMaxResources;

        public ResourceProductionComponent(ConstructionItem ci, Level level) : base(ci)
        {
            this.m_vTimeSinceLastClick = level.GetTime();
            this.m_vProductionResourceData = ObjectManager.DataTables.GetResourceByName(((BuildingData)ci.GetData()).ProducesResource);
            this.m_vResourcesPerHour = ((BuildingData)ci.GetData()).ResourcePerHour;
            this.m_vMaxResources = ((BuildingData)ci.GetData()).ResourceMax;
        }

        public void CollectResources()
        {

            ConstructionItem ci = (ConstructionItem)GetParent();
            TimeSpan span = ci.GetLevel().GetTime() - this.m_vTimeSinceLastClick;
            float currentResources = ((this.m_vResourcesPerHour[ci.UpgradeLevel] / (60f * 60f)) * (float)(span.TotalSeconds));

            currentResources = Math.Min(Math.Max(currentResources, 0), this.m_vMaxResources[ci.UpgradeLevel]);

            if (currentResources >= 10)
            {
                ClientAvatar ca = ci.GetLevel().GetPlayerAvatar();
                if (ca.GetResourceCap(this.m_vProductionResourceData) >= ca.GetResourceCount(this.m_vProductionResourceData) + currentResources)
                {
                    ca.CommodityCountChangeHelper(0, this.m_vProductionResourceData, (int)currentResources);
                    this.m_vTimeSinceLastClick = ci.GetLevel().GetTime();
                }
            }
        }

        public override void Load(JObject jsonObject)
        {
            JObject productionObject = (JObject)jsonObject["production"];
            if (productionObject != null)
            {
                this.m_vTimeSinceLastClick = productionObject["t_lastClick"].ToObject<DateTime>();
            }
        }

        public override JObject Save(JObject jsonObject)
        {
            JObject productionObject = new JObject();
            productionObject.Add("t_lastClick", this.m_vTimeSinceLastClick);
            jsonObject.Add("production", productionObject);
            jsonObject.Add("res_time", (GetParent().GetLevel().GetTime() - this.m_vTimeSinceLastClick).TotalSeconds);

            return jsonObject;
        }
    }
}
