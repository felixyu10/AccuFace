angular.module('myFaceApp', [])

.controller('faceDetection', function ($scope, FileUploadService) {

    $scope.DetectedResultsMessage = '';
    $scope.SelectedFileForUpload = null;
    $scope.Uploaded = [];
    $scope.SimilarFace = [];
    $scope.FaceRectangles = [];
    $scope.DetectedFaces = [];

    //File Select & Save 
    $scope.selectCandidateFileforUpload = function (file) {
        gtag('event', 'PhotoDetection');
        $('.loadmore').css("display", "block");
        $scope.SelectedFileForUpload = file;
        $scope.loaderMoreupl = true;
        $scope.uplMessage = '上傳中...';
        $scope.result = "color-green";

        //Save File
        var uploaderUrl = "/FaceDetection/UploadPhoto";
        var fileSave = FileUploadService.UploadFile($scope.SelectedFileForUpload, uploaderUrl);
        fileSave.then(function (response) {
            if (response.data.Status) {
                $scope.GetDetectedFaces();
                angular.forEach(angular.element("input[type='file']"), function (inputElem) {
                    angular.element(inputElem).val(null);
                });
                $scope.f1.$setPristine();
                $scope.uplMessage = response.data.Message;
                $scope.loaderMoreupl = false;
            }
        },
        function (error) {
        alert("UploadFile 發生錯誤: " + error);
        });
    }

    //Get Detected Faces
    $scope.GetDetectedFaces = function () {

        $scope.loaderMore = true;
        $scope.faceMessage = '辨識中，請稍後...';
        $scope.result = "color-green";

        var fileUrl = "/FaceDetection/GetDetectedFaces";
        var fileView = FileUploadService.GetUploadedFile(fileUrl);

        fileView.then(function (response) {
            alert(response);
            $('.loadmore').css("display", "none");
            $('.facePreview_hr').css("display", "block");
            $scope.QueryFace = response.data.QueryFaceImage;
            $scope.DetectedResultsMessage = response.data.DetectedResults;
            $scope.DetectedFaces = response.data.FaceInfo;
            $scope.FaceRectangles = response.data.FaceRectangles;
            $scope.loaderMore = false;

            $('#faceCanvas_img').remove();
            $('.Rectangle_box').remove();

            //get element byID
            var canvas = document.getElementById('faceCanvas');

            //add image element
            var elemImg = document.createElement("img");
            elemImg.setAttribute("src", $scope.QueryFace);
            elemImg.setAttribute("width", "95%");
            elemImg.id = 'faceCanvas_img';
            canvas.append(elemImg);

            //Loop with face rectangles
            angular.forEach($scope.FaceRectangles, function (imgs, i) {
                //console.log($scope.DetectedFaces[i])
                //Create rectangle for every face
                var divRectangle = document.createElement('div');
                var width = imgs.Width;
                var height = imgs.Height;
                var top = imgs.Top;
                var left = imgs.Left;

                var percent = $("#faceCanvas_img").width() / 450;
                //Style Div
                divRectangle.className = 'Rectangle_box';
                divRectangle.style.width = width * percent + 'px';
                divRectangle.style.height = height * percent + 'px';
                divRectangle.style.top = top * percent + 'px';
                divRectangle.style.left = left * percent + 'px';
                divRectangle.id = 'Rectangle_' + (i + 1);

                //Generate rectangles
                canvas.append(divRectangle);
            });
        },
        function (error) {
            alert("GetDetectedFaces 發生錯誤: " + error);
        });
    };

    $scope.hoverIn = function () {
        this.hoverEdit = true;
    };
})

.controller('webcam', function ($scope, FileUploadService) {

    var width;
    var height;

    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        width = $(window).width();
        height = width * 0.8;
    }
    else {
        if ($(window).width() > 480) {
            width = 480;
            height = 400;
        }
        else {
            width = $(window).width() - 30;
            height = width * 0.8;
        }
    }

    //if ($(window).width() >= 768) {
    //    width = 480;
    //    height = 400;
    //}
    //else {
    //    width = 400;
    //    height = 320;
    //}

    $(window).load(function () {
        Webcam.set({
            width: width,
            height: height,
            image_format: 'jpeg',
            jpeg_quality: 100,
            fps: 60
        });
        Webcam.attach('#webcam');

    });

    $(window).resize(function () {
        if ($(window).width() > 480) {
            width = 480;
            height = 400;
        }
        else {
            width = $(window).width() - 30;
            height = width * 0.8;
        }
        $("video").css("width", width + "px")
        $("video").css("height", height + "px")

        $("#webcam").css("width", width + "px")
        $("#webcam").css("height", height + "px")
    });

    $scope.DetectedResultsMessage = '';
    $scope.SelectedFileForUpload = null;
    $scope.Uploaded = [];
    $scope.SimilarFace = [];
    $scope.FaceRectangles = [];
    $scope.DetectedFaces = [];

    //File Select & Save 
    $scope.Capture = function (e) {

        //Webcam截圖
        Webcam.snap(function (data_uri) {
            gtag('event', 'WebcamDetection');
            var uploaderUrl = "/FaceDetection/Capture";
            var capture = FileUploadService.Capture(uploaderUrl, data_uri);
            capture.then(function (response) {

                if (response.data.Status) {
                    $scope.GetDetectedFaces();
                    angular.forEach(angular.element("input[type='file']"), function (inputElem) {
                        angular.element(inputElem).val(null);
                    });
                    //$scope.f1.$setPristine();
                    $scope.uplMessage = response.data.Message;
                    $scope.loaderMoreupl = true;
                    $scope.uplMessage = '偵測中...';
                }
            },
            function (error) {
                console.warn("Error: " + error);
            });
        });
    }


    //Get Detected Faces
    $scope.GetDetectedFaces = function () {
        $scope.loaderMore = true;
        $scope.faceMessage = '辨識人臉中，請稍後...';
        $scope.result = "color-green";

        var fileUrl = "/FaceDetection/GetDetectedFaces";
        var fileView = FileUploadService.GetUploadedFile(fileUrl);

        fileView.then(function (response) {
            $('.loadmore').css("display", "none");
            $('.facePreview_hr').css("display", "block");
            $scope.QueryFace = response.data.QueryFaceImage;
            $scope.DetectedResultsMessage = response.data.DetectedResults;
            $scope.DetectedFaces = response.data.FaceInfo;
            $scope.FaceRectangles = response.data.FaceRectangles;
            $scope.loaderMore = false;

            //Reset element
            $('#faceCanvas_img').remove();
            $('.Rectangle_box').remove();


            //get element byID
            var canvas = document.getElementById('faceCanvas');

            if (canvas != null) {
                //add image element
                var elemImg = document.createElement("img");
                elemImg.setAttribute("src", $scope.QueryFace);
                elemImg.setAttribute("width", "95%");
                elemImg.id = 'faceCanvas_img';
                canvas.append(elemImg);


                //Loop with face rectangles
                angular.forEach($scope.FaceRectangles, function (imgs, i) {
                    //console.log($scope.DetectedFaces[i])
                    //Create rectangle for every face
                    var divRectangle = document.createElement('div');
                    var width = imgs.Width;
                    var height = imgs.Height;
                    var top = imgs.Top;
                    var left = imgs.Left;

                    var percent = $("#faceCanvas_img").width() / 450;
                    //Style Div
                    divRectangle.className = 'Rectangle_box';
                    divRectangle.style.width = width * percent + 'px';
                    divRectangle.style.height = height * percent + 'px';
                    divRectangle.style.top = top * percent + 'px';
                    divRectangle.style.left = left * percent + 'px';
                    divRectangle.id = 'Rectangle_' + (i + 1);

                    //Generate rectangles
                    canvas.append(divRectangle);
                });
            }
        },
        function (error) {
            console.warn("Error: " + error);
        });
    };

    $scope.hoverIn = function () {
        this.hoverEdit = true;
    };
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

    fact.Capture = function (uploaderUrl, data_uri) {
        var formData = new FormData();
        formData.append("imgUrl", data_uri);
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
        return $http.get(fileUrl);
    }
    return fact;
})
