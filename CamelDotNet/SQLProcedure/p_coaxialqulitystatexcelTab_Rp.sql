create procedure [dbo].[p_coaxialqualitystatexcelTab_Rp]
	@testtimestart datetime2,
	@testtimestop datetime2,
	@drillingcrew nvarchar(50),--机台
	@workgroup nvarchar(50) --班组
as
begin
	SET NOCOUNT ON;
	declare @vantotal_result table
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
		VnaRecordId_Result int,
		LossPercent_Result decimal(8,2),
		TestItemId_Fail int,
		TestItemName_Fail nvarchar(50),
		ProcessId_Fail int,
		ProcessName_Fail nvarchar(50),
		QualityLossId_Result int,
		QualityLossPercentId_Result int,
		FreqFormularR nvarchar(50),
		ValueFormularR nvarchar(50)
	)
	insert into @vantotal_result
		exec p_coaxialqualitystatTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup
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
		--TotalFail group length
		select 
			a.TestDate,
			a.DrillingCrew,
			a.WorkGroup,
			a.ProductFullName,
			a.Price,
			SUM(a.Lengths)/1000 as TotalFailLength --unit to km
		from @vantotal_result a
		where a.TestResult = 1
		group by a.TestDate,a.DrillingCrew,a.WorkGroup,a.ProductFullName,a.Price
		) aa
		--Total group length
		join
		(
			select 
				a.TestDate,
				a.DrillingCrew,
				a.WorkGroup,
				a.ProductFullName,
				a.Price,
				SUM(a.Lengths)/1000 as TotalLength -- unit to km
			from @vantotal_result a
			group by a.TestDate,a.DrillingCrew,a.WorkGroup,a.ProductFullName,a.Price
		) aaa
		on aa.TestDate = aaa.TestDate
		and	aa.DrillingCrew = aaa.DrillingCrew
		and aa.WorkGroup = aaa.WorkGroup
		and aa.ProductFullName = aaa.ProductFullName
		and aa.Price = aaa.Price

	--vantotal_result_failgroup_formular
	select 
		a.TestDate,
		a.DrillingCrew,
		a.WorkGroup,
		a.ProductFullName,
		b.TotalLength,
		b.TotalFailLength,
		b.PassPercent,
		b.Price,
		a.TestItemName_Fail,
		a.ProcessName_Fail,
		a.QualityLossId_Result,
		a.QualityLossPercentId_Result,
		a.FreqFormularR,
		a.ValueFormularR,
		a.PerFailLength
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
			CAST(SUM(a.Lengths)/1000 as decimal(8,2)) as PerFailLength -- unit to km
		from @vantotal_result a where a.TestResult = 1
		group by a.TestDate,a.DrillingCrew,a.WorkGroup,a.ProductFullName,a.TestItemName_Fail,a.ProcessName_Fail,a.QualityLossId_Result,a.QualityLossPercentId_Result,a.FreqFormularR,a.ValueFormularR
	) a
	join
	@vantotal_result_failgroup b
	on a.TestDate = b.TestDate
	and a.DrillingCrew = b.DrillingCrew
	and a.WorkGroup = b.WorkGroup
	and a.ProductFullName = b.ProductFullName
end