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

import { NavigationActions,HeaderBackButton } from 'react-navigation'
import GestureRecognizer, {swipeDirections} from 'react-native-swipe-gestures';

function renderLeft(params,navigation) {
if(params == undefined) return(<Text> </Text>);

  if(params.breadcrumb != undefined && (params.filePath != undefined && params.filePath != "/")) {
    return (<HeaderBackButton onPress={() => navigation.navigate('Home', {
      title: params ? params.breadcrumbName : "dsf",
      filePath: params ? params.breadcrumb : "/",
      })} />)
  }
  else {
    return(<Text> </Text>)
  }
}

export default class ArchiveScreen extends React.Component {
  constructor(props){
    super(props);
    const { params } = this.props.navigation.state;
    this.state = { 
       isLoading: true,
       filePath: params ? params.filePath : "/",
      //  title: params ? params.title : "Home"
      }
  }
  
  static navigationOptions = ({ navigation, navigationOptions }) => {
    const { params } = navigation.state;

    var title = params ? params.title : 'Home';
    if(title === undefined) title = "Starsky"
    return {
      title: title,
      headerLeft: renderLeft(params,navigation),
      // headerLeft: (<HeaderBackButton onPress={() => navigation.goBack(null)} />),
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
          <View style={styles.flatlist}>
            <FlatList
              data={this.state.dataSource.fileIndexItems}
              renderItem={({item}) => 
              <View style={styles.flatlistItem}>
                <Text
                  onPress={() => {
                    /* 1. Navigate to the Details route with params */
                    this.props.navigation.navigate('Home', {
                      title: item.fileName,
                      filePath: item.filePath,
                    });
                  }}
                > 
                  {item.fileName}
                </Text>

                <Image
                  style={styles.detailviewthumb}
                  // source={{uri: thumbBasePath + item.fileHash}}
                  source={{uri: "https://blog.solutotlv.com/wp-content/uploads/2017/03/article_screen_sizes-1280x447.png"}}
                />

              </View>}
                keyExtractor={(item, index) => index}
            />
          </View>
          <View style={styles.nextprev}>
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

    console.log("filePathfilePath");
    console.log(filePath);

    var filePath = this.state.filePath;
    if(filePath === undefined) filePath = "/";
    var filePath = filePath.replace(/ /ig,"$20");

    // http://localhost:5000/?json=true&f= 


    return fetch('https://qdraw.eu/starsky_tmp_access_894ikrfs8m438g/api/f=' + filePath)
      .then((response) => response.json())
      .then((responseJson) => {

        var breadcrumb = responseJson.breadcrumb[responseJson.breadcrumb.length-1];
        var breadcrumbName = breadcrumb.split("/")[breadcrumb.split("/").length-1];
        if(breadcrumbName === "")  breadcrumbName = "Starsky";

        this.props.navigation.setParams({ 
          breadcrumb: breadcrumb,
          breadcrumbName: breadcrumbName,
        });

        this.setState({
          isLoading: false,
          dataSource: responseJson,
          fileName: responseJson.searchQuery,

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
    backgroundColor: 'gray',
    marginTop: 10,
    justifyContent: 'center',
    alignItems: 'center'
  },
  detailviewthumb: {
    width: 50,
    height: 50,
  },
  flatlist: {
    flex: 10,
    flexWrap: 'wrap', 
    alignItems: 'flex-start',
    flexDirection: 'row',
    justifyContent: 'space-between'
  },
  flatlistItem: {
    // width: 50,
    // backgroundColor: 'yellow',
    // flex:1
  },
  nextprev: {
    flex: 1,
    flexWrap: 'wrap', 
    alignItems: 'flex-start',
    flexDirection: 'row',
    justifyContent: 'space-between'
    // flexDirection:'row',
    // backgroundColor: 'yellow',
  },
  nextprev_button: {
    // width: 30,
    // height: 30,
    // flex: 2,
    height: Math.round(Dimensions.get('window').height/12),
    width: Math.round(Dimensions.get('window').width/2),
    // flexDirection: 'row',
    // justifyContent: 'space-between'
  },
  nextprev_button_prev: {
    backgroundColor: 'yellow'
  },
  nextprev_button_next: {
    backgroundColor: 'red'
  }
});






