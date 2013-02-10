'use strict';
var SpeakerNet;
(function (SpeakerNet) {
    angular.module('SpeakerNet.Voting', [
        'SpeakerNet.VotingServices'
    ]).config(function ($httpProvider) {
        $httpProvider.defaults.transformRequest.push(SpeakerNet.Transformations.AddAntiForgeryTokenToRequest);
    }).controller("Voting", SpeakerNet.VotingController).controller("Result", SpeakerNet.ResultController);
})(SpeakerNet || (SpeakerNet = {}));
//@ sourceMappingURL=SessionVoting.js.map
