angular.module('myFaceApp', [])

.controller('User', function ($scope, FileUploadService) {

    $scope.DetectedResultsMessage = '';
    $scope.SelectedFileForUpload = null;
    $scope.Uploaded = [];
    $scope.SimilarFace = [];
    $scope.FaceRectangles = [];
    $scope.DetectedFaces = [];

    //File Select & Save 
    $scope.selectCandidateFileforUpload = function ($file) {

        $('.loadmore').css("display", "block");
        $scope.SelectedFileForUpload = $file;
        $scope.loaderMoreupl = true;
        $scope.uplMessage = '新增中...';
        $scope.result = "color-green";

        if ($("[name=userName]").val() == "") {
            alertify.error("請輸入名稱");
            $("[name=file]").val("")
            return;
        }
        else {
            try {
                var uploaderUrl = "/User/Create?userName=" + $("[name=userName]").val();
                var fileSave = FileUploadService.UploadFile($scope.SelectedFileForUpload, uploaderUrl);
                fileSave.then(function (response) {
                    alert("Status: " + response.data.Status)
                    $scope.loaderMoreupl = false;
                    $("[name=userName]").val("")
                    $("[name=file]").val("")
                    if (response.data.Status) {
                        alertify.success("新增成功");
                    }
                    else {
                        alertify.error("發生錯誤");
                    }
                }, function (error) {
                    alert("Fail: " + error);
                });
            }
            catch (e) {
                console.log("Got an error!", e);
                throw e; // rethrow to not marked as handled
            }
        }
    }
})
.factory('FileUploadService', function ($http, $q) {
    var fact = {};
    fact.UploadFile = function (files, uploaderUrl) {
        var formData = new FormData();
        angular.forEach(files, function (f, i) {
            formData.append("file", files[i]);
        });
        var request = $http({
            method: "post",
            url: uploaderUrl,
            data: formData,
            withCredentials: true,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        return request;
    }
    fact.GetUploadedFile = function (fileUrl) {
        alert('fileUrl:' + fileUrl)
        return $http.get(fileUrl);
    }
    return fact;
})