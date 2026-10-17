// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
; (function (SiteJs, $) {
  var _self = SiteJs;

  _self.UpdateDropdown = function (listItem) {
    document.querySelector('.dropdown-toggle').innerText = listItem.text;
  }

})(window.SiteJs = window.SiteJs || {}, jQuery);
