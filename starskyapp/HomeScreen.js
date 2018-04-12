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


  _prevButton() {
    var prev =  this.state.dataSource.relativeObjects.prevFilePath;
    if(prev != null) {
      return (
        <Button
          title={"Prev"}
          onPress={() => {
            /* 1. Navigate to the Details route with params */
            this.props.navigation.navigate('Home', {
              title: "item.fileName",
              filePath: prev,
            });
          }}
        />
      );  
    }
  }

  _nextButton() {
    var next =  this.state.dataSource.relativeObjects.nextFilePath;
    if(next != null) {
      return (
        <Button
          title={"Next"}
          onPress={() => {
            /* 1. Navigate to the Details route with params */
            this.props.navigation.navigate('Home', {
              title: "item.fileName",
              filePath: next,
            });
          }}
        />
      );  
    }
  }

  _renderScene() {

    if (this.state.dataSource.pageType === 'Archive') {

      var thumbBasePath = 'https://qdraw.eu/starsky_tmp_access_894ikrfs8m438g/api/f=';

      return(
        <View style={{flex: 1, paddingTop:20}}>
          <View>
            {this._prevButton()}
            {this._nextButton()}
          </View>
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

      );
    }
    if (this.state.dataSource.pageType === 'DetailView') {
      var thumbPath = 'https://qdraw.eu/starsky_tmp_access_894ikrfs8m438g/thumbnail?f=' + this.state.dataSource.fileIndexItem.fileHash
      return(
        <ScrollView>
          <View>
            {this._prevButton()}
            {this._nextButton()}
          </View>


          <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center' }}>
            <Text>otherParam: {JSON.stringify(this.state.dataSource.fileIndexItem.fileName)}</Text>
            <Image
              style={styles.stretch}
              source={{uri: thumbPath}}
            />

          </View>
        </ScrollView>
      )
    }
 }
  
  componentDidMount(){

    // http://localhost:5000/?json=true&f= 
    return fetch('https://qdraw.eu/starsky_tmp_access_894ikrfs8m438g/api/f=' + this.state.filePath)
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






