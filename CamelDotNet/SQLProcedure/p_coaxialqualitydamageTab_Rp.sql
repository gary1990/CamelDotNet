create procedure [dbo].[p_coaxialqualitydamageTab_Rp]
	@testtimestart datetime2,
	@testtimestop datetime2,
	@drillingcrew nvarchar(50),--机台
	@workgroup nvarchar(50), --班组
	@producttypeId int
as
begin
	SET NOCOUNT ON;
	--判断@producttypeId，如果为0，置为null
	IF @producttypeId = 0
	BEGIN
		SET @producttypeId = NULL
	END

	DECLARE @vantotal TABLE
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
		IsGreenLight int
	)
	--质量损失比定义
	declare @qulityloss Table
	(
		ProcessId int,
		TestItemId int,
		QualityLossFreq nvarchar(50),
		FreqFormularR nvarchar(50),
		FreqFormular nvarchar(50),
		QualityLossRef nvarchar(50),
		ValueFormularR nvarchar(50),
		ValueFormular nvarchar(50),
		LossPercent decimal(11,5),
		QualityLossId int,
		QualityLossPercntId int,
		FormularLevel int
	)

	insert into @vantotal
		select 
			ROW_NUMBER() OVER(ORDER BY aa.VnaRecordId) AS RowNumber,--为每条记录生成RowNumber，供循环使用
			aa.*
		from
		(
			select 
			a.Id as VnaRecordId,
			a.TestTime,
			a.TestTime as TestDate,
			a.DrillingCrew,
			a.WorkGroup,
			a.TestResult,
			a.InnerLength,
			ABS(a.OuterLength-a.InnerLength) as Lengths,
			a.OuterLength,
			a.TestStationId,
			d.Name as TestStaion,
			d.ProcessId as TestStationProcessId,
			e.Name as TestStationProcess,
			c.Id as ProductTypeId,
			c.Price as Price,
			c.Name+'#'+c.ModelName as ProductFullName, 
			b.Number as SerialNum,
			a.isGreenLight as IsGreenLight
			from VnaRecord a
			join SerialNumber b
			on a.SerialNumberId = b.Id
			join ProductType c
			on a.ProductTypeId = c.Id
			join TestStation d
			on a.TestStationId = d.Id
			join Process e
			on d.ProcessId = e.Id

			where 
			a.NoStatistics = 0 --wipe out NoStatistics
			and a.TestTime >= @testtimestart
			and a.TestTime <= @testtimestop
			and a.DrillingCrew like '%' + IsNull(@drillingcrew,a.DrillingCrew) +'%'
			and a.WorkGroup like '%' + IsNull(@workgroup,a.workgroup) + '%'
			and a.ProductTypeId = ISNULL(@producttypeId,a.ProductTypeId)
		) aa
		where aa.TestTime in
		(
			select 
			MAX(a.TestTime) as TestTime
			from 
			VnaRecord a
			where 
			a.NoStatistics = 0 --wipe out NoStatistics
			and a.TestTime >= @testtimestart
			and a.TestTime <= @testtimestop
			and a.DrillingCrew like '%' + IsNull(@drillingcrew,a.DrillingCrew) +'%'
			and a.WorkGroup like '%' + IsNull(@workgroup,a.workgroup) + '%'
			and a.ProductTypeId = ISNULL(@producttypeId,a.ProductTypeId)
			group by a.SerialNumberId
		)

	insert into @qulityloss
		select 
		aa.*,
		CASE 
			WHEN aa.FreqFormular like '%or%' THEN 2 --2 is for multiple， eg (xxx,xxx)(xxx,xxx)
			WHEN aa.FreqFormular like '%is not nu%' THEN 3 -- 3 is for all
			ELSE 1 --1 is for single
		END FormularLevel
		from
		(
			select 
				a.ProcessId,
				a.TestItemId,
				b.QualityLossFreq,
				CASE 
					WHEN RTRIM(LTRIM(b.QualityLossFreq)) = '(,)' THEN '(-∞, +∞)'--(,) due to all, equals (-∞, +∞)
					WHEN RTRIM(LTRIM(b.QualityLossFreq)) is null THEN '(-∞, +∞)'--(,) due to all, equals (-∞, +∞)
					ELSE REPLACE(REPLACE(RTRIM(LTRIM(b.QualityLossFreq)), ',', '-'), ')(', ')且(')
				END FreqFormularR,
				CASE 
					WHEN RTRIM(LTRIM(b.QualityLossFreq)) = '(,)' THEN ' fv is not null '--(,) due to all, equals (-∞, +∞)
					WHEN RTRIM(LTRIM(b.QualityLossFreq)) is null THEN ' fv is not null '--(,) due to all, equals (-∞, +∞)
					ELSE REPLACE(REPLACE(RTRIM(LTRIM(b.QualityLossFreq)), ',', ' <= fv and fv <= '), ')(', ' or ')--replace to result ( xxx <= fv and fv <= xxx) or (xxx <= fv and fv <= xxx or xxx <= fv and fv <= xxx)
				END FreqFormular,
				b.QualityLossRef,
				CASE 
					WHEN RTRIM(LTRIM(b.QualityLossRef)) = '(,)' THEN '(-∞, +∞)'
					WHEN RTRIM(LTRIM(b.QualityLossRef)) is null THEN '(-∞, +∞)'
					ELSE REPLACE(REPLACE(REPLACE(RTRIM(LTRIM(b.QualityLossRef)), '(,', '(<'), ',)','<)'), ',', '-')
				END ValueFormularR,
				CASE 
					WHEN RTRIM(LTRIM(b.QualityLossRef)) = '(,)' THEN ' vv is not null '
					WHEN RTRIM(LTRIM(b.QualityLossRef)) is null THEN ' vv is not null '
					ELSE REPLACE(REPLACE(REPLACE(RTRIM(LTRIM(b.QualityLossRef)), '(,', ' ( vv < '), ',)',' < vv )'), ',', ' < vv and vv <')
				END ValueFormular,
				b.LossValue,
				a.Id as QualityLossId,
				b.Id as QualityLossPercntId
			from QualityLoss a
			join QualityLossPercent b
			on b.QualityLossId = a.Id
		) aa
--select * from @qulityloss

	declare @per_van_fail_details_formular_table table
	(
		Rownumber INT IDENTITY(1,1),
		VnaRecordId int,
		TestStationProcessId int,
		TestStationProcess nvarchar(50),
		TestItemIdVna int,
		TestItemResult int,
		XValue decimal(18,2),
		YValue decimal(18,2),
		RValue decimal(18,2),
		TestitemPerResult int,
		TestItemName nvarchar(50),
		TestItemCategoryId int,
		ProcessId int,
		TestItemId int,
		QualityLossFreq nvarchar(50),
		FreqFormular nvarchar(50),
		QualityLossRef nvarchar(50),
		ValueFormular nvarchar(50),
		LossPercent nvarchar(50),
		FormularLevel int,
		QualityLossId int,
		QualityLossPercntId int,
		FreqFormularR nvarchar(50),
		ValueFormularR nvarchar(50)
	);
	--get total number of selected vnarecord
	declare @vnatotalnum int
	select @vnatotalnum = MAX(RowNumber) from @vantotal
	declare @rowNo int
	set @rowNo = 1
	--while for @vnatotal
	while @rowNo <= @vnatotalnum
	begin
		declare @vnarecordid int
		--get vanrecordid in this row
		select @vnarecordid = a.VnaRecordId from @vantotal a where a.RowNumber = @rowNo;
		with per_van_fail_details
		as
		(	
			select 
			d.VnaRecordId,
			a.Id as VnaTestItemRecordId,
			d.TestStationProcessId,
			d.TestStationProcess,
			a.TestItemId as TestItemIdVna, 
			a.TestItemResult,
			b.Id as VnaTestItemPerRecordId,
			case
				when b.XValue < 0.001 then b.XValue*1000000000 -- if XValue < 0.001, XValue's unit is s, *1000000000 to ns
				when b.XValue > 0.001 then b.XValue/1000000000 -- if XVakue > 0.001, XValue's unit is Freq, /1000000000 to M Freq
				when b.XValue is null then 0
				else b.XValue
			end XValue,
			case
				when b.YValue is null then 0
				else b.YValue
			end YValue,
			b.RValue,b.TestitemPerResult, c.Name as TestItemName, c.TestItemCategoryId
			from VnaTestItemRecord a
			left join VnaTestItemPerRecord b
			on b.VnaTestItemRecordId = a.Id
			join TestItem c
			on a.TestItemId = c.Id
			join @vantotal d
			on d.VnaRecordId = a.VnaRecordId
			where a.TestItemResult = 1 -- get failed record
			and (b.TestitemPerResult = 1 or b.TestitemPerResult is null)
			and a.VnaRecordId = @vnarecordid
		),
		per_van_fail_details_formular as
		(
			select * from per_van_fail_details a
			left join @qulityloss b
			on a.TestItemIdVna = b.TestItemId
			and a.TestStationProcessId = b.ProcessId
		)
		insert into @per_van_fail_details_formular_table
		(
			VnaRecordId,
			TestStationProcessId,
			TestStationProcess,
			TestItemIdVna,
			TestItemResult,
			XValue,
			YValue,
			RValue,
			TestitemPerResult,
			TestItemName,
			TestItemCategoryId,
			ProcessId,
			TestItemId,
			QualityLossFreq,
			FreqFormular,
			QualityLossRef,
			ValueFormular,
			LossPercent,
			FormularLevel,
			QualityLossId,
			QualityLossPercntId,
			FreqFormularR,
			ValueFormularR
		)
		select 
			a.VnaRecordId,
			a.TestStationProcessId,
			a.TestStationProcess,
			a.TestItemIdVna,
			a.TestItemResult,
			a.XValue,
			a.YValue,
			a.RValue,
			a.TestitemPerResult,
			a.TestItemName,
			a.TestItemCategoryId,
			a.ProcessId,
			a.TestItemId,
			a.QualityLossFreq,
			a.FreqFormular,
			a.QualityLossRef,
			a.ValueFormular,
			a.LossPercent,
			a.FormularLevel,
			a.QualityLossId,
			a.QualityLossPercntId,
			a.FreqFormularR,
			a.ValueFormularR
		from per_van_fail_details_formular a 
		set @rowNo=@rowNo+1
	end

--select * from @per_van_fail_details_formular_table

	declare @van_fail_detail_result table
	(
		VnaProcessId int,
		VnaProcessName nvarchar(50),
		XValue decimal(18,2),
		YValue decimal(18,2),
		FreqFormular nvarchar(255),
		ValueFormular nvarchar(255),
		LossPercent decimal(18,5),
		FormularLevel int,
		VnaRecordId int,
		TestItemId int,
		TestItemName nvarchar(50),
		PerResult int,
		QualityLossId int,
		QualityLossPercntId int,
		FreqFormularR nvarchar(50),
		ValueFormularR nvarchar(50)
	)

	--get total number of selected vnarecord
	declare @per_van_fail_details_formular_table_rows int
	select @per_van_fail_details_formular_table_rows = MAX(Rownumber) from @per_van_fail_details_formular_table
	declare @rowNo_temp1 int
	set @rowNo_temp1 = 1
	--while for @vnatotal
	while @rowNo_temp1 <= @per_van_fail_details_formular_table_rows
	begin
		declare @vnaprocessid_result int
		declare @vnaprocessname_result nvarchar(50)
		declare @xvalue_result decimal(18,2)
		declare @yvalue_result decimal(18,2)
		declare @fformular_result nvarchar(255)
		declare @vformular_result nvarchar(255)
		declare @losspercent_result decimal(18,5)
		declare @formularlevel_result int
		declare @vnarecordid_result int
		declare @testitemid_result int
		declare @testitemname_result nvarchar(50)
		declare @qualitylossid int
		declare @qualitylosspercentid int
		declare @freqFormularR nvarchar(50)
		declare @valueFormularR nvarchar(50)
		select
			@vnaprocessid_result = a.TestStationProcessId,
			@vnaprocessname_result = a.TestStationProcess,
			@xvalue_result = a.XValue,
			@yvalue_result = a.YValue,
			@fformular_result = REPLACE(a.FreqFormular, 'fv', a.XValue),
			@vformular_result = REPLACE(a.ValueFormular,'vv', a.YValue),
			@losspercent_result = a.LossPercent,
			@formularlevel_result = a.FormularLevel,
			@vnarecordid_result = a.VnaRecordId,
			@testitemid_result = a.TestItemId,
			@testitemname_result = a.TestItemName,
			@qualitylossid = a.QualityLossId,
			@qualitylosspercentid = a.QualityLossPercntId,
			@freqFormularR = a.FreqFormularR,
			@valueFormularR = a.ValueFormularR
		from @per_van_fail_details_formular_table a where a.Rownumber = @rowNo_temp1

		--if @losspercent_result is not null
		--begin
			declare @sql nvarchar(max)
			declare @result int
			set @sql = N'set @result = ' +'IIF(' + @fformular_result + ' and ' + @vformular_result+', 0, 1)'--0 is matched, 1 is not matched
			exec sp_executesql @sql, N'@result int output', @result out
			insert into @van_fail_detail_result
			values
			(
				@vnaprocessid_result,
				@vnaprocessname_result,
				@xvalue_result, 
				@yvalue_result, 
				@fformular_result, 
				@vformular_result,
				@losspercent_result,
				@formularlevel_result,
				@vnarecordid_result,
				@testitemid_result,
				@testitemname_result,
				@result,
				@qualitylossid,
				@qualitylosspercentid,
				@freqFormularR,
				@valueFormularR
			)
		--end
		set @rowNo_temp1=@rowNo_temp1+1
	end
	--total false result
	declare @vnatotal_result table
	(
		VnaRecordId_Result int,
		LossPercent_Result decimal(11,5),
		TestItemId_Fail int,
		TestItemName_Fail nvarchar(50),
		ProcessId_Fail int,
		ProcessName_Fail nvarchar(50),
		QualityLossId_Result int,
		QualityLossPercentId_Result int,
		FreqFormularR nvarchar(50),
		ValueFormularR nvarchar(50)
	)

	declare @van_fail_detail_result_group table
	(
		MatchedNum int,
		VnaRecordId int, 
		LossPercent decimal(11,5),
		FormularLevel int,
		TestItemId int,
		TestItemName nvarchar(50),
		VnaProcessId int,
		VnaProcessName nvarchar(50),
		QualityLossId int,
		QualityLossPercntId int,
		FreqFormularR nvarchar(50),
		ValueFormularR varchar(50)
	)

	declare @van_fail_detail_result_per_group table
	(
		MatchedNum int,
		VnaRecordId int, 
		LossPercent decimal(11,5),
		FormularLevel int,
		TestItemId int,
		TestItemName nvarchar(50),
		VnaProcessId int,
		VnaProcessName nvarchar(50),
		QualityLossId int,
		QualityLossPercntId int,
		FreqFormularR nvarchar(50),
		ValueFormularR varchar(50)
	)

	declare @van_fail_detail_result_loss table
	(
		MatchedNum int,
		VnaRecordId int, 
		LossPercent decimal(11,5),
		FormularLevel int,
		TestItemId int,
		TestItemName nvarchar(50),
		VnaProcessId int,
		VnaProcessName nvarchar(50),
		QualityLossId int,
		QualityLossPercntId int,
		FreqFormularR nvarchar(50),
		ValueFormularR varchar(50)
	)

	insert into @van_fail_detail_result_group
		select count(*) as MatchedNum,a.VnaRecordId, a.LossPercent,a.FormularLevel,a.TestItemId,a.TestItemName,a.VnaProcessId,a.VnaProcessName,a.QualityLossId,a.QualityLossPercntId,a.FreqFormularR,a.ValueFormularR
		from @van_fail_detail_result a 
		where (a.PerResult = 0 or a.PerResult is null)
		group by a.VnaRecordId, a.LossPercent,a.FormularLevel,a.TestItemId,a.TestItemName,a.VnaProcessId,a.VnaProcessName,a.QualityLossId,a.QualityLossPercntId,a.FreqFormularR,a.ValueFormularR

	insert into @van_fail_detail_result_per_group
		select 
			c.*
		from
		(
			select
			aa.MatchedNum,
			aa.TestItemName,
			aa.VnaRecordId,
			aa.VnaProcessId,
			aa.VnaProcessName,
			MIN(b.FormularLevel) as FormularLevel
			from
			(
				select MAX(a.MatchedNum) as MatchedNum,a.VnaRecordId,a.TestItemName,a.VnaProcessId,a.VnaProcessName
				from @van_fail_detail_result_group a
				group by a.VnaRecordId,a.TestItemName,a.VnaProcessId,a.VnaProcessName
			) aa
			left join @van_fail_detail_result_group b
			on
			aa.VnaRecordId = b.VnaRecordId
			and aa.TestItemName = b.TestItemName
			and aa.MatchedNum = b.MatchedNum
			group by aa.MatchedNum, aa.TestItemName,aa.VnaRecordId,aa.VnaProcessId,aa.VnaProcessName
		) aaa
		inner join @van_fail_detail_result_group c
		on
		(coalesce(aaa.MatchedNum,'') = coalesce(c.MatchedNum,''))
		and (coalesce(aaa.TestItemName,'') = coalesce(c.TestItemName,''))
		and (coalesce(aaa.VnaRecordId,'') = coalesce(c.VnaRecordId,''))
		and (coalesce(aaa.VnaProcessId,'') = coalesce(c.VnaProcessId,''))
		and (coalesce(aaa.VnaProcessName,'') = coalesce(c.VnaProcessName,''))
		and (coalesce(aaa.FormularLevel,'') = coalesce(c.FormularLevel,''))

	insert into @van_fail_detail_result_loss
		select b.*
		from
		(
			select MAX(a.LossPercent) as LossPercent,a.VnaRecordId,a.VnaProcessId
			from @van_fail_detail_result_per_group a
			group by a.VnaRecordId,a.VnaProcessId
		) aa
		left join @van_fail_detail_result_per_group b
		on 
		(coalesce(aa.LossPercent,0.1) = coalesce(b.LossPercent,0.1))
		and (coalesce(aa.VnaRecordId,'') = coalesce(b.VnaRecordId,''))
		and (aa.VnaProcessId = b.VnaProcessId)
--select * from @van_fail_detail_result_loss
	insert into @vnatotal_result
		select
			a.VnaRecordId,
			a.LossPercent,
			a.TestItemId,
			a.TestItemName,
			a.VnaProcessId,
			a.VnaProcessName,
			a.QualityLossId,
			a.QualityLossPercntId,
			a.FreqFormularR,
			a.ValueFormularR
		from @van_fail_detail_result_loss a
--select * from @vnatotal_result
	select 
	aaa.*,
	bbb.Id as DepartmentId,
	bbb.Name as DepartmentName,
	CAST((aaa.Lengths/1000)*aaa.Price*aaa.LossPercent_Result as decimal(8,2)) as LossMoney
	from
	(
		select aa.* from
		(
			select 
				a.*,b.*
			from @vantotal a
			left join @vnatotal_result b
			on a.VnaRecordId = b.VnaRecordId_Result
		) aa
		where
		(aa.ProcessName_Fail is null and aa.TestResult = 0)--get pass record
		or
		(aa.ProcessName_Fail is not null)--get fail record
		or
		(aa.ProcessName_Fail is not null and aa.IsGreenLight = 1)--get greenlight record
	) aaa
	left join
	QualityPassRecord bb
	on aaa.VnaRecordId = bb.VnaRecordId
	left join Department bbb
	on bb.DepartmentId = bbb.Id
end