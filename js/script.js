$(document).ready(function () {
    $.sy_ajaxyukle = function (secenekler) {
        var secenek = $.extend({
            servis: "",
            params: "{}",
            doldur: ""
        }, secenekler);
        AjaxIslemleri.yukle(secenek.servis, secenek.params, secenek.doldur);
    };
    $.sy_ajaxislemyukle = function (secenekler) {
        var secenek = $.extend({
            servis: "",
            params: "{}",
            doldur: ""
        }, secenekler);
        AjaxIslemleri.single_yukle(secenek.servis, secenek.params, secenek.doldur);
    };
    $.sy_ajaxjsonyukle = function (secenekler) {
        var secenek = $.extend({
            servis: "",
            params: "{}",
            doldur: ""
        }, secenekler);
        AjaxIslemleri.json_veri(secenek.servis, secenek.params, secenek.doldur);
    };
});
$(document).ajaxStart(function () {

});
$(document).ajaxStop(function () {

});
$(document).ajaxError(function (event, jqxhr, settings, thrownError) {
    alert("İstek işlenirken bir hata oluştu hata detayları için konsola bakınız.");
    console.log(event.handleObj.origType + " >> İstek işlenirken bir hata oluştu: " + jqxhr.status + " " + jqxhr + "<br>" + thrownError + "<br>" + settings.url, 10000);
    console.log(jqxhr);
    console.log(event);
    console.log(thrownError);
});
var sy = {
    kodolustur: function (uzunluk) {
        var text = "";
        var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        for (var i = 0; i < uzunluk; i++)
            text += possible.charAt(Math.floor(Math.random() * possible.length));
        return text;
    },
    metinkisalt: function (metin, uzunluk) {
        if (metin != undefined) {
            var karakter = uzunluk;
            var sinir = "...";
            var c, h, baslik;
            if (metin.length > karakter) {
                c = metin.substr(0, karakter);
                h = metin.substr(karakter - 1, metin.length - karakter);
                baslik = c + sinir;
                return baslik;
            } else {
                baslik = metin;
                return baslik;
            }
        }
    },

    ResolveClientUrl: function (url) {
        var base_url = window.location.hostname;
        if (base_url.indexOf("localhost") != -1) {
            base_url = location.protocol + "//" + window.location.hostname + ":" + window.location.port + "/";
        } else {
            base_url = location.protocol + "//" + window.location.hostname + "/";
        }
        var doc = document
       , old_base = doc.getElementsByTagName('base')[0]
       , old_href = old_base && old_base.href
       , doc_head = doc.head || doc.getElementsByTagName('head')[0]
       , our_base = old_base || doc_head.appendChild(doc.createElement('base'))
       , resolver = doc.createElement('a')
       , resolved_url
        ;
        our_base.href = base_url;
        resolver.href = url;
        resolved_url = resolver.href;
        if (old_base) old_base.href = old_href;
        else doc_head.removeChild(our_base);
        return resolved_url;
    },

}
var AjaxIslemleri = {
    yukle: function (servis, params, doldur) {
        $.ajax({
            async: false,
            type: "POST",
            url: servis,
            data: params,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $(msg.d).each(function () {
                    var ayir = this.split('~');
                    doldur(ayir);
                });
            },
            error: function (msg) {
                console.log((new Error).lineNumber + ": İçerik yüklenemedi! Eksik veya hatalı parametre!" + msg);
            }
        });
    },
    single_yukle: function (servis, params, doldur) {
        $.ajax({
            async: false,
            type: "POST",
            url: servis,
            data: params,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                doldur(msg.d);
            },
            error: function (msg) {
                console.log((new Error).lineNumber + ": İçerik yüklenemedi! Eksik veya hatalı parametre!" + msg);
            }
        });
    },
    json_veri: function (servis, params, doldur) {
        $.ajax({
            async: false,
            type: "POST",
            url: servis,
            data: params,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var obje = jQuery.parseJSON(msg.d);
                for (var i = 0; i < obje.length; i++) {
                    doldur(obje[i]);
                }
            },
            error: function (msg) {
                console.log((new Error).lineNumber + ": İçerik yüklenemedi! Eksik veya hatalı parametre!" + msg);
            }
        });
    }
}