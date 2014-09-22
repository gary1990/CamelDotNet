create procedure [dbo].[p_coaxialqualitydamage_departmentTab_Rp]
@testtimestart datetime2,
	@testtimestop datetime2,
	@drillingcrew nvarchar(50),--机台
	@workgroup nvarchar(50), --班组
	@producttypeId int
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
		LossPercent_Result decimal(11,5),
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
		exec p_coaxialqualitydamageTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId

	select 
		aaa.ProductTypeId,
		aaa.ProductFullName,
		SUM(aaa.LossMoney) as ProductLossMoney,
		aaa.DepartmentId,
		aaa.DepartmentName,
		aaa.DepartmentLossMoney
	from
	(
		select
			aa.ProductTypeId,
			aa.ProductFullName,
			aa.DepartmentId,
			aa.DepartmentName,
			aa.LossMoney,
			bb.DepartmentLossMoney
		from
		(
			select
				a.*
			from @vantotal_demage_result a
			where a.DepartmentName is not null
		) aa
		join
		(
			select 
				a.DepartmentId,a.DepartmentName,
				SUM(a.LossMoney) as DepartmentLossMoney
			from @vantotal_demage_result a
			where a.DepartmentName is not null
			group by a.DepartmentId,a.DepartmentName
		) bb
		on aa.DepartmentId = bb.DepartmentId
		and aa.DepartmentName = bb.DepartmentName
	) aaa
	group by 
	aaa.ProductTypeId,aaa.ProductFullName,aaa.DepartmentId,aaa.DepartmentName,aaa.DepartmentLossMoney
	order by aaa.DepartmentName

end