create procedure [dbo].[p_coaxialqualitydamageexcelTab_Rp]
	@testtimestart datetime2,
	@testtimestop datetime2,
	@drillingcrew nvarchar(50),--机台
	@workgroup nvarchar(50) --班组
as
begin
	SET NOCOUNT ON;	
	declare @vantotal_demage_result table
	(
		RowNumber int,
		VnaRecordId int,
		TestTime datetime2(0),
		TestDate date,
		DrillingCrew nvarchar(50),
		WorkGroup nvarchar(50),
		TestResult int,
		InnerLength decimal(8,2),
		Lengths decimal(8,2),
		OuterLength decimal(8,2),
		TestStationId int,
		TestStaion nvarchar(50),
		TestStationProcessId int,
		TestStationProcess nvarchar(50),
		ProductTypeId int,
		Price decimal(8,2),
		ProductFullName nvarchar(100), 
		SerialNum nvarchar(100),
		IsGreenLight int,
		VnaRecordId_Result int,
		LossPercent_Result decimal(8,2),
		TestItemId_Fail int,
		TestItemName_Fail nvarchar(50),
		ProcessId_Fail int,
		ProcessName_Fail nvarchar(50),
		QualityLossId_Result int,
		QualityLossPercentId_Result int,
		FreqFormularR nvarchar(50),
		ValueFormularR nvarchar(50),
		DepartmentId int,
		DepartmentName varchar(20),
		LossMoney decimal(18,2)
	)

	insert into @vantotal_demage_result
		exec p_coaxialqualitydamageTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup
	
	declare @vantotal_result_failgroup table
	(
		TestDate date,
		DrillingCrew nvarchar(50),
		WorkGroup nvarchar(50),
		ProductFullName nvarchar(100),
		TotalLength decimal(8,2),
		TotalFailLength decimal(8,2),
		PassPercent decimal(5,2),
		Price decimal(8,2)
	)

	insert into @vantotal_result_failgroup
		--取得两个分组的分组各项及两个分组的长度合计，并计算出合格率
		select
			aa.TestDate,
			aa.DrillingCrew,
			aa.WorkGroup,
			aa.ProductFullName,
			aaa.TotalLength,
			aa.TotalFailLength,
			100-(aa.TotalFailLength/aaa.TotalLength)*100 as PassPercent,
			aa.Price
		from
		(
			--根据日期、机台、班组、物料名称分组，并取得（结果中不合格或放行为合格）分组各项及分组的长度合计（换算为km）
			select 
				a.TestDate,
				a.DrillingCrew,
				a.WorkGroup,
				a.ProductFullName,
				a.Price,
				SUM(a.Lengths)/1000 as TotalFailLength --unit to km
			from @vantotal_demage_result a where a.TestResult = 1 or (a.TestResult = 0 and a.IsGreenLight = 1)--pass result,but is green light
			group by a.TestDate,a.DrillingCrew,a.WorkGroup,a.ProductFullName,a.Price
		) aa
		join
		(
			--根据日期、机台、班组、物料名称分组，并取得分组各项及分组的长度合计（换算为km）
			select 
				a.TestDate,
				a.DrillingCrew,
				a.WorkGroup,
				a.ProductFullName,
				a.Price,
				SUM(a.Lengths)/1000 as TotalLength -- unit to km
			from @vantotal_demage_result a
			group by a.TestDate,a.DrillingCrew,a.WorkGroup,a.ProductFullName,a.Price
		) aaa
		on aa.TestDate = aaa.TestDate
		and	aa.DrillingCrew = aaa.DrillingCrew
		and aa.WorkGroup = aaa.WorkGroup
		and aa.ProductFullName = aaa.ProductFullName
		and aa.Price = aaa.Price

	select 
		aa.TestDate,
		aa.DrillingCrew,
		aa.WorkGroup,
		aa.ProductFullName,
	
		bb.TotalLength,
		bb.TotalFailLength,
		bb.PassPercent,
		bb.Price,

		aa.TestItemName_Fail,
		aa.ProcessName_Fail,
		aa.QualityLossId_Result,
		aa.QualityLossPercentId_Result,
		aa.FreqFormularR,
		aa.ValueFormularR,
		aa.PerLossLength,

		aa.DepartmentId,
		aa.DepartmentName,
		aa.PerLossMoney
	from
	(
		select 
			a.TestDate,
			a.DrillingCrew,
			a.WorkGroup,
			a.ProductFullName,

			a.TestItemName_Fail,
			a.ProcessName_Fail,
			a.QualityLossId_Result,
			a.QualityLossPercentId_Result,
			a.FreqFormularR,
			a.ValueFormularR,
			CAST(SUM(a.Lengths)/100 as decimal(8,2)) as PerLossLength,

			a.DepartmentId,
			a.DepartmentName,
			CAST(SUM(a.LossMoney) as decimal(18,2)) as PerLossMoney
		from @vantotal_demage_result a where a.TestResult = 1 or (a.TestResult = 0 and a.IsGreenLight = 1)--pass result,but is green light
		group by a.TestDate,a.DrillingCrew,a.WorkGroup,a.ProductFullName,a.TestItemName_Fail,a.ProcessName_Fail,a.QualityLossId_Result,a.QualityLossPercentId_Result,a.FreqFormularR,a.ValueFormularR,a.DepartmentId,a.DepartmentName,a.LossMoney
	) aa
	join
	@vantotal_result_failgroup bb
	on aa.TestDate = bb.TestDate
	and aa.DrillingCrew = bb.DrillingCrew
	and aa.WorkGroup = bb.WorkGroup
	and aa.ProductFullName = bb.ProductFullName
end