using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

[Table("OutputStd")]
public class OutputStd
{
	public int ID { get; set; }

	public string SWATVersion { get; set; }
	public double TotalArea { get; set; }
	public double Precipitation { get; set; }
	public double SurfaceRunoffQ { get; set; }
	public double LateralSoilQ { get; set; }
	public double GroundWaterQ { get; set; }
	public double Revap { get; set; }
	public double DeepAQRecharge { get; set; }
	public double TotalAQRecharge { get; set; }
	public double ET { get; set; }
	public double PET { get; set; }
	public double WaterStressDays { get; set; }
	public double TemperatureStressDays { get; set; }
	public double NStressDays { get; set; }
	public double PStressDays { get; set; }
	public double InitNO3 { get; set; }
	public double FinalNO3 { get; set; }
	public double InitOrgN { get; set; }
	public double FinalOrgN { get; set; }
	public double HumusMinOrgN { get; set; }
	public double ActiveToStableOrgN { get; set; }
	public double MinFromFreshOrgN { get; set; }
	public double NO3InFert { get; set; }
	public double AmmoniaInFert { get; set; }
	public double OrgNInFert { get; set; }
	public double AmmoniaVolatilization { get; set; }
	public double AmmoniaNitrification { get; set; }
	public double Denitrification { get; set; }
	public double NUptake { get; set; }
	public double PUptake { get; set; }
	public double ActiveToSolutionPFlow { get; set; }
	public double ActiveToStablePFlow { get; set; }
	public double PFertApplied { get; set; }
	public double NFertApplied { get; set; }
	public double HumusMinOrgP { get; set; }
	public double MinFromFreshOrgP { get; set; }
	public double MineralPInFert { get; set; }
	public double OrgPInFert { get; set; }
	public double InitMinP { get; set; }
	public double FinalMinP { get; set; }
	public double InitOrgP { get; set; }
	public double FinalOrgP { get; set; }
	public double OrgN { get; set; }
	public double OrgP { get; set; }
	public double NO3YieldSQ { get; set; }
	public double NO3YieldLat { get; set; }
	public double SolPYield { get; set; }
	public double NO3Leached { get; set; }
	public double NO3YieldGWQ { get; set; }
	public double NRemovedInYield { get; set; }
	public double PRemovedInYield { get; set; }
	public double TotalSedimentLoading { get; set; }
}
