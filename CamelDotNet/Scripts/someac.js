//////
$(document).ready(function () {
    // 顶部操作ul，若无操作文字，则删除li
    aculblankdelete();
    // 次级菜单中，前shownum个链接显示，后面的都隐藏在li.more中，并且为li.more绑定click事件。
    // 该方法于加载页面时使用，shownum默认为6
    menushowmore(6);
    // 操作集合，找到所有类名为groupOperation，
    // 若其中action不多于actionnum个，则不作改变，
    // 多于，则将所有操作都放在groupOperation中的ul.inline-list中

    groupactions(1);
});

// 显示隐藏的各个操作
function showac(a) {
    $(a).next().toggle();

}

// 次级菜单中，前shownum个链接显示，后面的都隐藏在li.more中，并且为li.more绑定click事件。
// 该方法于加载页面时使用，shownum默认为6
function menushowmore(shownum) {
    if (!shownum) {
        shownum = 6;
    }
    $('#menu ul.sideMenu').each(function () {
        if ($(this).children().length > shownum) {
            $(this).append('<li class="more"><ul></ul></li>');
            $(this).children('li.more').children('ul').append($(this).children('li:gt(' + (shownum - 1) + ')[class!="more"]'));

        }
    });
    $('#menu ul.sideMenu li.more').click(function (event) {
        if ($(event.target).hasClass('more'))
            $(this).children().toggle();
    });
}

// 操作集合，找到所有类名为groupOperation，
// 若其中action不多于actionnum个，则不作改变，
// 多于，则将所有操作都放在groupOperation中的ul.inline-list中
function groupactions(actionnum) {
    if (!actionnum) {
        actionnum = 2;
    }
    $('.groupOperation').each(function () {
        if ($(this).children('a').length > actionnum) {
            var htm = $(this).html();
            $(this).html('<a class="button blue" href="javascript: void(0);"><.........></a>'
			    + '<ul class="inline-list" style="display:none;">'
				+ htm.replace(/<a /g, '<li><a ').replace(/<\/a>/g, '</a></li>')
				+ '</ul>');
            $(this).hover(function () { showac($(this).children('a')[0]); }, function () { showac($(this).children('a')[0]); });
        }
    });
}

// 顶部操作ul，若无操作文字，则删除li
function aculblankdelete() {
    $('ul.acul>li>ul>li').each(function () {
        if ($.trim($(this).html()) == '') {
            $(this).remove();
        }
    });
}