import React, { Component } from 'react';
import {
  Platform,
  StyleSheet,
  Text,
  View,
  SectionList,
  Image,
  FlatList,
  ActivityIndicator,
  ScrollView,
  Button,
  Dimensions
} from 'react-native';

import { NavigationActions } from 'react-navigation'
import GestureRecognizer, {swipeDirections} from 'react-native-swipe-gestures';

export default class ArchiveScreen extends React.Component {
  constructor(props){
    super(props);
    const { params } = this.props.navigation.state;
    this.state = { 
       isLoading: true,
       filePath: params ? params.filePath : "/",
       fileName: params ? params.filePath : ""
      }
  }
  
  static navigationOptions = ({ navigation, navigationOptions }) => {
    const { params } = navigation.state;
    return {
      title: params ? params.title : 'Home',
      filePath : params ? params.filePath : '/',
    };
  };

  onSwipeLeft(gestureState) {
    var next =  this.state.dataSource.relativeObjects.nextFilePath;
    if(next != null) {
      this.props.navigation.navigate('Home', {
        title: next.split("/")[next.split("/").length-1],
        filePath: next,
      },
      {
        transitionStyle: 'inverted'
      }
      );
    }
  }
 
  onSwipeRight(gestureState) {
    var prev =  this.state.dataSource.relativeObjects.prevFilePath;
    if(prev != null) {
      this.props.navigation.navigate('Home', {
        title: prev.split("/")[prev.split("/").length-1],
        filePath: prev,
      });
    }
  }

  // _prevButton() {
  //   var prev =  this.state.dataSource.relativeObjects.prevFilePath;
  //   if(prev != null) {
  //     return (
  //       <Button
  //         title={"Prev"}
  //         onPress={() => {
  //           /* 1. Navigate to the Details route with params */
  //           this.props.navigation.navigate('Home', {
  //             title: "item.fileName",
  //             filePath: prev,
  //           });
  //         }}
  //       />
  //     );  
  //   }
  // }

  // // {this._prevButton()}
  // _nextButton() {
  //   var next =  this.state.dataSource.relativeObjects.nextFilePath;
  //   if(next != null) {
  //     return (
  //       <Button
  //         title={"Next"}
  //         onPress={() => {
  //           /* 1. Navigate to the Details route with params */
  //           this.props.navigation.navigate('Home', {
  //             title: "item.fileName",
  //             filePath: next,
  //           });
  //         }}
  //       />
  //     );  
  //   }
  // }

  _renderScene() {
    const config = {
      velocityThreshold: 0.3,
      directionalOffsetThreshold: 80
    };

    if (this.state.dataSource.pageType === 'Archive') {

      var thumbBasePath = 'https://qdraw.eu/starsky_tmp_access_894ikrfs8m438g/api/f=';

      return(
        <GestureRecognizer
        onSwipeLeft={(state) => this.onSwipeLeft(state)}
        onSwipeRight={(state) => this.onSwipeRight(state)}
        config={config}
        style={{
          flex: 1
        }}
        >
          <View style={{flex: 1, paddingTop:20}}>
            <FlatList
              data={this.state.dataSource.fileIndexItems}
              renderItem={({item}) => 
              <Text>
                <Image
                  style={styles.detailviewthumb}
                  source={{uri: thumbBasePath + item.fileHash}}
                />
                <Button
                  title={"Go to " + item.fileName}
                  onPress={() => {
                    /* 1. Navigate to the Details route with params */
                    this.props.navigation.navigate('Home', {
                      title: item.fileName,
                      filePath: item.filePath,
                    });
                  }}
                />
              </Text>}
                keyExtractor={(item, index) => index}
            />
          </View>
        </GestureRecognizer>

      );
    }
    if (this.state.dataSource.pageType === 'DetailView') {
      var thumbPath = 'https://qdraw.eu/starsky_tmp_access_894ikrfs8m438g/thumbnail?f=' + this.state.dataSource.fileIndexItem.fileHash
      return(
        <GestureRecognizer
        onSwipeLeft={(state) => this.onSwipeLeft(state)}
        onSwipeRight={(state) => this.onSwipeRight(state)}
        config={config}
        style={{
          flex: 1
        }}
        >
          <ScrollView>
            <View>
            </View>

            <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center' }}>
              <Text>otherParam: {JSON.stringify(this.state.dataSource.fileIndexItem.fileName)}</Text>
              <Image
                style={styles.stretch}
                source={{uri: thumbPath}}
              />

            </View>
          </ScrollView>
        </GestureRecognizer>

      )
    }
 }
  
  componentDidMount(){

    // http://localhost:5000/?json=true&f= 
    var filePath = this.state.filePath.replace(/ /ig,"$20");

    // console.log("filePathfilePath");
    // console.log(filePath);

    return fetch('https://qdraw.eu/starsky_tmp_access_894ikrfs8m438g/api/f=' + filePath)
      .then((response) => response.json())
      .then((responseJson) => {

        this.setState({
          isLoading: false,
          dataSource: responseJson,
          fileName: responseJson.searchQuery
        }, function(){
        });

      })
      .catch((error) =>{
        console.error(error);
      });
  }

  render(){
    
     if(this.state.isLoading){
      return(
        <View style={{flex: 1, padding: 20}}>
          <ActivityIndicator/>
        </View>
      )
    }

    return this._renderScene()

  }
}

var styles = StyleSheet.create({
  stretch: {
    width: Dimensions.get('window').width,
    height: 300,
    backgroundColor: 'red',
    marginTop: 10,
    justifyContent: 'center',
    alignItems: 'center'
  },
  detailviewthumb: {
    width: 50,
    height: 50,
  }
});






